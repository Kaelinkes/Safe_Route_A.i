"""Fast API smoke tests for the existing model contract. Run: pytest -q"""
from fastapi.testclient import TestClient
from serve_risk import app
from weather_mapper import map_weather, map_lighting, kmh_to_model_speed

client = TestClient(app)

def segment(**overrides):
    value = {"road_type": "urban", "num_lanes": 2, "curvature": 0.05,
             "speed_limit": kmh_to_model_speed(60), "lighting": "daylight", "weather": "clear",
             "road_signs_present": True, "public_road": True, "time_of_day": "afternoon",
             "holiday": False, "school_season": True, "num_reported_accidents": 0}
    value.update(overrides)
    return value

def test_health():
    r = client.get("/health")
    assert r.status_code == 200
    assert len(r.json()["features"]) == 12

def test_valid_prediction_and_explanations():
    r = client.post("/predict", json={"segments": [segment()]})
    assert r.status_code == 200
    result = r.json()["results"][0]
    assert result["status"] == "Valid"
    assert result["riskLevel"] in {"Low", "Medium", "High"}
    assert "explanations" in result

def test_missing_feature_is_not_guessed():
    s = segment(); del s["weather"]
    result = client.post("/predict", json={"segments": [s]}).json()["results"][0]
    assert result["status"] == "InsufficientData"
    assert "weather" in result["missingFields"]

def test_invalid_category_rejected():
    result = client.post("/predict", json={"segments": [segment(weather="storm")]}).json()["results"][0]
    assert result["status"] == "InvalidInput"
    assert "weather" in result["invalidFields"]

def test_negative_numeric_rejected():
    result = client.post("/predict", json={"segments": [segment(num_lanes=-1)]}).json()["results"][0]
    assert result["status"] == "InvalidInput"

def test_empty_batch_rejected():
    assert client.post("/predict", json={"segments": []}).status_code == 422

def test_weather_mapping():
    assert map_weather(45) == "foggy"
    assert map_weather(61) == "rainy"
    assert map_weather(0) == "clear"
    assert map_weather(None) is None
    assert map_lighting(0) == "night"
    assert map_lighting(None) is None

def test_speed_conversion_does_not_clip_120_and_130():
    a, b = kmh_to_model_speed(120), kmh_to_model_speed(130)
    assert b > a
    assert round(a, 1) == 74.6
    assert round(b, 1) == 80.8

def test_approved_community_report_overlay():
    from community_risk import score_approved_reports
    reports = [{"approved": True, "severity": "high", "reported_at": "2026-11-06T10:00:00Z", "distance_m": 50},
               {"approved": False, "severity": "critical", "reported_at": "2026-11-06T10:00:00Z", "distance_m": 10}]
    score = score_approved_reports(reports, now="2026-11-06T11:00:00Z")
    assert score["approvedReportsUsed"] == 1
    assert score["score"] > 0

def test_open_meteo_route_endpoint_uses_each_point(monkeypatch):
    import serve_risk
    async def fake_weather(points):
        assert len(points) == 2
        return [{"weather": "clear", "lighting": "daylight"},
                {"weather": "rainy", "lighting": "night"}]
    monkeypatch.setattr(serve_risk, "fetch_route_weather", fake_weather)
    a, b = segment(), segment()
    for item in (a, b):
        item.pop("weather"); item.pop("lighting")
    a.update(latitude=-26.2, longitude=28.0, speed_limit_kmh=60)
    b.update(latitude=-25.75, longitude=28.19, speed_limit_kmh=120)
    response = client.post("/predict-route", json={"segments": [a, b]})
    assert response.status_code == 200
    body = response.json()
    assert body["routePointsProcessed"] == 2
    assert body["results"][0]["status"] == "Valid"
    assert body["results"][1]["status"] == "Valid"
    assert body["results"][1]["warnings"]  # 120 km/h is out of legacy training range


def test_no_community_reports_do_not_change_ml_score():
    from community_risk import blend_risk
    assert blend_risk(0.4, None, 0.15) == 0.4
    assert blend_risk(0.4, 0.0, 0.15) == 0.4
    assert blend_risk(0.4, 0.8, 0.15) >= 0.4
