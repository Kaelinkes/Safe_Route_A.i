from feature_enrichment.model_adapter import predict_risk

sample = {
    "road_type": "urban",
    "num_lanes": 2,
    "curvature": 0.2,
    "speed_limit": 60,
    "lighting": "daylight",
    "weather": "clear",
    "road_signs_present": True,
    "public_road": True,
    "time_of_day": "afternoon",
    "holiday": False,
    "school_season": True,
    "num_reported_accidents": 0
}

print(predict_risk(sample))
