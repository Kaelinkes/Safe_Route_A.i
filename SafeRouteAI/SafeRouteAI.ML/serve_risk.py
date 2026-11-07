"""SafeRoute AI prediction API. Legacy bundle remains backward compatible.

POST /predict       -> predictions for prepared model features
POST /predict-route -> fetches Open-Meteo at every route segment, then predicts
GET  /health        -> model and feature contract
"""
from __future__ import annotations
import os
import math
import httpx
import joblib
import numpy as np
import pandas as pd
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from community_risk import score_approved_reports, blend_risk
from weather_mapper import kmh_to_model_speed, speed_range_warning
from weather_client import fetch_route_weather

MODEL_PATH = os.environ.get("RISK_MODEL", "saferoute_s5e10_model.joblib")
BUNDLE = joblib.load(MODEL_PATH)
MODEL, FEATURES = BUNDLE["model"], BUNDLE["features"]
CATS, TH, LEVELS = BUNDLE["categories"], BUNDLE["thresholds"], BUNDLE["levels"]
MARGIN = float(os.environ.get("BORDERLINE_MARGIN", "0.04"))
COMMUNITY_ALPHA = float(os.environ.get("COMMUNITY_RISK_ALPHA", "0.15"))
CUTS = [TH["low_max"], TH["medium_max"]]
TRAIN_SPEED_MIN = float(os.environ.get("MODEL_SPEED_MIN", "25"))
TRAIN_SPEED_MAX = float(os.environ.get("MODEL_SPEED_MAX", "70"))

app = FastAPI(title="SafeRoute AI risk model", version="2.0.0")

class Batch(BaseModel):
    segments: list[dict] = Field(min_length=1, max_length=500)

class RouteBatch(BaseModel):
    # Each item contains latitude, longitude, arrival_time (optional), model features
    segments: list[dict] = Field(min_length=1, max_length=100)


def _is_missing(v):
    if v is None:
        return True
    if isinstance(v, (float, np.floating)) and not math.isfinite(float(v)):
        return True
    return False


def _validate_segment(seg):
    missing = [f for f in FEATURES if f not in seg or _is_missing(seg.get(f))]
    invalid = []
    for f in FEATURES:
        if f in missing:
            continue
        v = seg[f]
        if f in CATS:
            if str(v).strip().lower() not in CATS[f]:
                invalid.append(f)
        elif f in ("road_signs_present", "public_road", "holiday", "school_season"):
            if not isinstance(v, (bool, int, np.integer)) or int(v) not in (0, 1):
                invalid.append(f)
        else:
            try:
                n = float(v)
                if not math.isfinite(n) or n < 0:
                    invalid.append(f)
            except (TypeError, ValueError):
                invalid.append(f)
    if "num_lanes" in seg and not _is_missing(seg.get("num_lanes")):
        try:
            if float(seg["num_lanes"]) < 1 or not float(seg["num_lanes"]).is_integer():
                invalid.append("num_lanes")
        except (TypeError, ValueError):
            invalid.append("num_lanes")
    if "curvature" in seg and not _is_missing(seg.get("curvature")):
        try:
            if float(seg["curvature"]) > 1:
                invalid.append("curvature")
        except (TypeError, ValueError):
            pass
    return sorted(set(missing)), sorted(set(invalid))


def _risk_level(score):
    return LEVELS[int(np.digitize(score, CUTS))]


def _explanations(df, row_index, community):
    reasons = []
    try:
        contributions = MODEL.predict(df.iloc[[row_index]], pred_contrib=True)[0]
        pairs = list(zip(FEATURES, contributions[:len(FEATURES)]))
        labels = {"curvature": "Road curvature", "speed_limit": "Speed limit (legacy model units)",
                  "weather": "Weather conditions", "lighting": "Lighting conditions",
                  "num_reported_accidents": "Previously reported accidents", "num_lanes": "Number of lanes",
                  "road_type": "Road type", "road_signs_present": "Road signs", "public_road": "Public road indicator",
                  "time_of_day": "Time of day", "holiday": "Holiday indicator", "school_season": "School-season indicator"}
        for name, value in sorted(pairs, key=lambda x: abs(float(x[1])), reverse=True)[:5]:
            if abs(float(value)) < 1e-6:
                continue
            reasons.append({"factor": labels.get(name, name), "direction": "increases" if value > 0 else "reduces",
                            "contribution": round(float(value), 5), "method": "LightGBM SHAP contribution"})
    except Exception:
        # Prediction remains available if the installed LightGBM version lacks pred_contrib.
        pass
    for item in community.get("explanations", []):
        reasons.append({"factor": item["type"], "direction": "increases", "method": "approved community report",
                        "severity": item["severity"], "distanceMetres": item["distanceMetres"], "ageHours": item["ageHours"]})
    return reasons


def _predict_segments(segments):
    results = [None] * len(segments)
    valid_indices, rows = [], []
    for i, seg in enumerate(segments):
        missing, invalid = _validate_segment(seg)
        if missing:
            results[i] = {"riskScore": None, "riskLevel": None, "status": "InsufficientData",
                          "missingFields": missing, "invalidFields": invalid, "explanations": []}
        elif invalid:
            results[i] = {"riskScore": None, "riskLevel": None, "status": "InvalidInput",
                          "missingFields": [], "invalidFields": invalid, "explanations": []}
        else:
            valid_indices.append(i)
            rows.append({f: seg[f] for f in FEATURES})
    if rows:
        df = pd.DataFrame(rows)
        for c, cats in CATS.items():
            df[c] = pd.Categorical(df[c].astype(str).str.lower().str.strip(), categories=cats)
        for c in ("road_signs_present", "public_road", "holiday", "school_season"):
            df[c] = df[c].astype("int8")
        model_scores = np.clip(MODEL.predict(df), 0, 1)
        for local_i, (original_i, ml_score) in enumerate(zip(valid_indices, model_scores)):
            seg = segments[original_i]
            community = score_approved_reports(seg.get("community_reports", []))
            community_component = community["score"] if community["approvedReportsUsed"] else None
            final_score = float(np.clip(blend_risk(ml_score, community_component, COMMUNITY_ALPHA), 0, 1))
            level = _risk_level(final_score)
            nearest = int(np.argmin([abs(final_score - c) for c in CUTS]))
            borderline = abs(final_score - CUTS[nearest]) < MARGIN
            other = LEVELS[nearest + 1] if level == LEVELS[nearest] else LEVELS[nearest]
            speed_warning = speed_range_warning(float(seg["speed_limit"]), TRAIN_SPEED_MIN, TRAIN_SPEED_MAX)
            warnings = [speed_warning] if speed_warning else []
            results[original_i] = {"riskScore": round(final_score, 4), "modelRiskScore": round(float(ml_score), 4),
                "communityRiskScore": community["score"] if community["approvedReportsUsed"] else None,
                "communityReportsUsed": community["approvedReportsUsed"],
                "riskLevel": level, "status": "Valid", "isBorderline": bool(borderline),
                "alsoPossible": other if borderline else None, "warnings": warnings,
                "explanations": _explanations(df, local_i, community)}
    return results


@app.get("/health")
def health():
    return {"status": "ok", "model": "LightGBM", "features": FEATURES, "categories": CATS,
            "thresholds": TH, "borderlineMargin": MARGIN,
            "speedLimitContract": "legacy bundle expects mph; km/h conversion is performed by /predict-route",
            "trainingSpeedRange": {"minimum": TRAIN_SPEED_MIN, "maximum": TRAIN_SPEED_MAX},
            "communityBlendAlpha": COMMUNITY_ALPHA}


@app.post("/predict")
def predict(batch: Batch):
    return {"results": _predict_segments(batch.segments)}


@app.post("/predict-route")
async def predict_route(batch: RouteBatch):
    """Fetch live/current or hourly forecast weather at every route segment coordinate."""
    prepared, weather_results = [], []
    points = []
    for i, seg in enumerate(batch.segments):
        if "latitude" not in seg or "longitude" not in seg:
            raise HTTPException(status_code=422, detail=f"segments[{i}] requires latitude and longitude")
        try:
            lat, lon = float(seg["latitude"]), float(seg["longitude"])
            if not (-90 <= lat <= 90 and -180 <= lon <= 180):
                raise ValueError("coordinates outside WGS84 ranges")
        except (TypeError, ValueError):
            raise HTTPException(status_code=422, detail=f"segments[{i}] has invalid latitude/longitude")
        points.append({"latitude": lat, "longitude": lon, "arrival_time": seg.get("arrival_time")})
    try:
        weather_results = await fetch_route_weather(points)
    except (httpx.HTTPError, ValueError, RuntimeError, TypeError) as exc:
        raise HTTPException(status_code=502, detail=f"Weather retrieval failed: {str(exc)}")
    for seg, weather in zip(batch.segments, weather_results):
        item = dict(seg)
        item["weather"] = weather.get("weather")
        item["lighting"] = weather.get("lighting")
        if "speed_limit_kmh" in item:
            item["speed_limit"] = kmh_to_model_speed(item.pop("speed_limit_kmh"))
        prepared.append(item)
    return {"results": _predict_segments(prepared), "weather": weather_results,
            "weatherProvider": "Open-Meteo", "routePointsProcessed": len(weather_results)}
