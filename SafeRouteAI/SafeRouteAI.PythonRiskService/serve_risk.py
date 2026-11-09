
"""SafeRoute AI - Live Road Risk Prediction API.

GET  /health
POST /predict
POST /predict-route

Uses the existing trained LightGBM model.
Does not retrain or modify the model.
"""

from __future__ import annotations

import os
import math
from datetime import date, datetime
from zoneinfo import ZoneInfo

import httpx
import joblib
import numpy as np
import pandas as pd

from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field

from community_risk import score_approved_reports, blend_risk
from weather_mapper import map_weather, map_lighting
from weather_client import fetch_route_weather
from dpme_road_client import fetch_route_roads
from osm_road_client import find_nearest_road


# =========================================================
# MODEL CONFIGURATION
# =========================================================

BASE_DIR = os.path.dirname(os.path.abspath(__file__))

MODEL_PATH = os.environ.get(
    "RISK_MODEL",
    os.path.join(BASE_DIR, "saferoute_s5e10_model.joblib")
)

TRAINING_DATA_PATH = os.environ.get(
    "RISK_TRAINING_DATA",
    os.path.join(BASE_DIR, "train.csv")
)

if not os.path.exists(MODEL_PATH):
    raise FileNotFoundError(f"Model not found: {MODEL_PATH}")

BUNDLE = joblib.load(MODEL_PATH)

MODEL = BUNDLE["model"]
FEATURES = list(BUNDLE["features"])
CATS = BUNDLE["categories"]
TH = BUNDLE["thresholds"]
LEVELS = BUNDLE["levels"]

MARGIN = float(os.environ.get("BORDERLINE_MARGIN", "0.04"))
COMMUNITY_ALPHA = float(
    os.environ.get("COMMUNITY_RISK_ALPHA", "0.15")
)

CUTS = [float(TH["low_max"]), float(TH["medium_max"])]

SA_TIMEZONE = ZoneInfo("Africa/Johannesburg")

BOOLEAN_FEATURES = {
    "road_signs_present",
    "public_road",
    "holiday",
    "school_season"
}

app = FastAPI(
    title="SafeRoute AI Risk Prediction API",
    description="Live South African road risk prediction using LightGBM.",
    version="3.0.0"
)


# =========================================================
# REQUEST MODELS
# =========================================================

class Batch(BaseModel):
    segments: list[dict] = Field(min_length=1, max_length=500)


class RouteBatch(BaseModel):
    segments: list[dict] = Field(min_length=1, max_length=100)


# =========================================================
# GENERAL HELPERS
# =========================================================

def _is_missing(value):
    if value is None or value is pd.NA or value is pd.NaT:
        return True

    try:
        result = pd.isna(value)
        if isinstance(result, (bool, np.bool_)):
            return bool(result)
    except (TypeError, ValueError):
        pass

    return False


def _safe_number(value):
    if _is_missing(value):
        return None

    try:
        number = float(value)
        return number if math.isfinite(number) else None
    except (TypeError, ValueError):
        return None


def _boolean_value(value):
    if isinstance(value, (bool, np.bool_)):
        return bool(value)

    if isinstance(value, (int, float, np.integer, np.floating)):
        if value in (0, 1):
            return bool(value)

    normalized = str(value).strip().lower()

    if normalized in {"true", "1", "yes"}:
        return True

    if normalized in {"false", "0", "no"}:
        return False

    return None


def _json_safe(value):
    if isinstance(value, np.generic):
        return _json_safe(value.item())

    if isinstance(value, np.ndarray):
        return [_json_safe(item) for item in value.tolist()]

    if isinstance(value, dict):
        return {
            str(key): _json_safe(item)
            for key, item in value.items()
        }

    if isinstance(value, (list, tuple, set)):
        return [_json_safe(item) for item in value]

    if isinstance(value, (datetime, date)):
        return value.isoformat()

    if _is_missing(value):
        return None

    if isinstance(value, float) and not math.isfinite(value):
        return None

    return value


def _road_attributes(record):
    if not isinstance(record, dict):
        return {}

    attributes = record.get("attributes")

    if isinstance(attributes, dict):
        return {**attributes, **record}

    return record


# =========================================================
# TRAINING FALLBACKS
# =========================================================

def _load_training_defaults():
    """Load median/mode fallbacks without retraining."""

    if not os.path.exists(TRAINING_DATA_PATH):
        print("WARNING: Training fallback dataset unavailable.")
        return {}

    try:
        df = pd.read_csv(
            TRAINING_DATA_PATH,
            usecols=lambda column: column in FEATURES,
            low_memory=False
        )
    except Exception as exc:
        print(f"Training fallback loading failed: {exc}")
        return {}

    defaults = {}

    for feature in FEATURES:
        if feature not in df.columns:
            continue

        values = df[feature].dropna()

        if values.empty:
            continue

        if feature in CATS:
            normalized = (
                values.astype(str)
                .str.strip()
                .str.lower()
            )

            normalized = normalized[
                normalized.isin(CATS[feature])
            ]

            if not normalized.empty:
                defaults[feature] = str(
                    normalized.mode().iloc[0]
                )

        elif feature in BOOLEAN_FEATURES:
            converted = [
                _boolean_value(value)
                for value in values
            ]

            converted = [
                value for value in converted
                if value is not None
            ]

            if converted:
                defaults[feature] = bool(
                    pd.Series(converted).mode().iloc[0]
                )

        else:
            numeric = pd.to_numeric(
                values,
                errors="coerce"
            ).dropna()

            if not numeric.empty:
                defaults[feature] = float(
                    numeric.median()
                )

    print(
        f"Loaded {len(defaults)}/{len(FEATURES)} "
        "training fallback features."
    )

    return defaults


TRAINING_DEFAULTS = _load_training_defaults()


# =========================================================
# SOUTH AFRICAN CALENDAR
# =========================================================

SA_PUBLIC_HOLIDAYS = {
    date(2026, 1, 1),
    date(2026, 3, 21),
    date(2026, 4, 3),
    date(2026, 4, 6),
    date(2026, 4, 27),
    date(2026, 5, 1),
    date(2026, 6, 16),
    date(2026, 8, 9),
    date(2026, 8, 10),
    date(2026, 9, 24),
    date(2026, 12, 16),
    date(2026, 12, 25),
    date(2026, 12, 26),
}

SA_SCHOOL_TERMS = {
    2026: [
        (date(2026, 1, 14), date(2026, 3, 27)),
        (date(2026, 4, 8), date(2026, 6, 26)),
        (date(2026, 7, 21), date(2026, 9, 23)),
        (date(2026, 10, 6), date(2026, 12, 9)),
    ]
}


def _local_arrival_datetime(segment):
    value = segment.get("arrival_time")

    if not value:
        return None

    try:
        parsed = datetime.fromisoformat(
            str(value).replace("Z", "+00:00")
        )

        if parsed.tzinfo is None:
            parsed = parsed.replace(tzinfo=SA_TIMEZONE)

        return parsed.astimezone(SA_TIMEZONE)

    except (TypeError, ValueError):
        return None


def _derive_time_of_day(segment):
    arrival = _local_arrival_datetime(segment)

    if arrival is None:
        return None

    if 5 <= arrival.hour < 12:
        return "morning"

    if 12 <= arrival.hour < 18:
        return "afternoon"

    return "evening"


def _derive_calendar_features(segment):
    arrival = _local_arrival_datetime(segment)

    if arrival is None:
        return {
            "holiday": None,
            "school_season": None
        }

    route_date = arrival.date()

    holiday = (
        route_date in SA_PUBLIC_HOLIDAYS
        if route_date.year == 2026
        else None
    )

    terms = SA_SCHOOL_TERMS.get(route_date.year)

    school_season = (
        any(start <= route_date <= end for start, end in terms)
        if terms is not None
        else None
    )

    return {
        "holiday": holiday,
        "school_season": school_season
    }


# =========================================================
# WEATHER ENRICHMENT
# =========================================================

def _prepare_weather_features(weather):
    weather = weather or {}

    mapped_weather = map_weather(
        weather.get("weatherCode"),
        weather.get("visibilityMetres"),
        weather.get("precipitationMm")
    )

    mapped_lighting = map_lighting(
        weather.get("isDay"),
        weather.get("shortwaveRadiationWm2")
    )

    return {
        "weather": (
            mapped_weather
            if mapped_weather is not None
            else weather.get("weather")
        ),
        "lighting": (
            mapped_lighting
            if mapped_lighting is not None
            else weather.get("lighting")
        )
    }


# =========================================================
# ROAD ENRICHMENT
# =========================================================

def _classify_road_type(value):
    if value is None:
        return None

    value = str(value).strip().lower()

    if value in CATS.get("road_type", []):
        return value

    mapping = {
        "motorway": "highway",
        "motorway_link": "highway",
        "trunk": "highway",
        "trunk_link": "highway",
        "primary": "highway",
        "primary_link": "highway",
        "secondary": "urban",
        "secondary_link": "urban",
        "tertiary": "urban",
        "tertiary_link": "urban",
        "residential": "urban",
        "living_street": "urban",
        "service": "urban",
        "road": "urban",
        "track": "rural",
        "unclassified": "rural",
    }

    result = mapping.get(value)

    return result if result in CATS.get("road_type", []) else None


def _osm_lane_count(road):
    attrs = _road_attributes(road)

    value = (
        attrs.get("model_num_lanes")
        or attrs.get("lanes_forward")
        or attrs.get("num_lanes")
        or attrs.get("lanes")
    )

    number = _safe_number(value)

    if (
        number is not None
        and number.is_integer()
        and 1 <= number <= 12
    ):
        return int(number)

    return None


def _lookup_osm(latitude, longitude):
    try:
        return find_nearest_road(
            latitude,
            longitude,
            radius_metres=150
        )
    except (OSError, ValueError, RuntimeError) as exc:
        print(f"OSM lookup failed: {exc}")
        return None


def _apply_ngda_features(item, road):
    attrs = _road_attributes(road)

    if not attrs:
        return

    candidates = (
        attrs.get("road_type_raw"),
        attrs.get("road_category_raw"),
        attrs.get("road_type"),
        attrs.get("road_category"),
        attrs.get("ROAD_TYPE"),
        attrs.get("ROAD_CATEGORY"),
    )

    for candidate in candidates:
        mapped = _classify_road_type(candidate)

        if mapped:
            item["road_type"] = mapped
            item["_road_type_source"] = "NGDA Gauteng Roads"
            break

    speed = (
        attrs.get("speed_limit_kmh")
        or attrs.get("SPEED_LIMIT")
        or attrs.get("speed_limit")
    )

    if speed is not None:
        item["_ngda_speed_limit_raw"] = speed


def _apply_osm_features(item, osm_road):
    attrs = _road_attributes(osm_road)

    if not attrs:
        return

    mapped_type = _classify_road_type(
        attrs.get("model_road_type")
        or attrs.get("road_type")
    )

    if mapped_type:
        item["road_type"] = mapped_type
        item["_road_type_source"] = "OpenStreetMap"

    lanes = _osm_lane_count(attrs)

    if lanes is not None:
        item["num_lanes"] = lanes
        item["_lanes_source"] = "OpenStreetMap lane tags"

    access = attrs.get("motor_vehicle")

    if access is None:
        access = attrs.get("access")

    if access is not None:
        access = str(access).strip().lower()

        if access in {"yes", "designated", "permissive"}:
            item["public_road"] = True
            item["_public_road_source"] = (
                "OpenStreetMap explicit access tag"
            )

        elif access in {"no", "private"}:
            item["public_road"] = False
            item["_public_road_source"] = (
                "OpenStreetMap explicit access tag"
            )

    # Do not feed unverified geometry curvature into the model.
    item["_osm_geometry_curvature"] = attrs.get(
        "model_curvature"
    )

    # Do not feed speed limits until training units are verified.
    item["_osm_speed_limit_kmh"] = (
        attrs.get("model_speed_limit_kmh")
        or attrs.get("speed_limit")
    )


# =========================================================
# FEATURE VALIDATION
# =========================================================

def _validate_segment(segment):
    missing = []
    invalid = []

    for feature in FEATURES:
        value = segment.get(feature)

        if feature not in segment or _is_missing(value):
            missing.append(feature)
            continue

        if feature in CATS:
            if str(value).strip().lower() not in CATS[feature]:
                invalid.append(feature)

        elif feature in BOOLEAN_FEATURES:
            if _boolean_value(value) is None:
                invalid.append(feature)

        else:
            number = _safe_number(value)

            if number is None or number < 0:
                invalid.append(feature)

            elif feature == "curvature" and number > 1:
                invalid.append(feature)

    lanes = _safe_number(segment.get("num_lanes"))

    if (
        "num_lanes" not in missing
        and (
            lanes is None
            or not lanes.is_integer()
            or not 1 <= lanes <= 12
        )
    ):
        invalid.append("num_lanes")

    return sorted(set(missing)), sorted(set(invalid))


# =========================================================
# MODEL FEATURE PREPARATION
# =========================================================

def _prepare_model_segment(segment):
    prepared = dict(segment)
    imputed_fields = []

    for feature in FEATURES:
        if feature not in prepared or _is_missing(prepared[feature]):
            fallback = TRAINING_DEFAULTS.get(feature)

            if fallback is not None:
                prepared[feature] = fallback
                imputed_fields.append(feature)

    return prepared, imputed_fields


# =========================================================
# FEATURE AUDIT
# =========================================================

def _feature_audit(
    segment,
    original_segment,
    road,
    weather,
    imputed_fields=None
):
    imputed_fields = set(imputed_fields or [])

    missing, invalid = _validate_segment(segment)

    expected_formats = {
        "road_type": "highway, rural, urban",
        "num_lanes": "Positive integer",
        "curvature": "Number between 0 and 1",
        "speed_limit": "Non-negative number; units unverified",
        "lighting": "daylight, dim, night",
        "weather": "clear, foggy, rainy",
        "road_signs_present": "Boolean or 0/1",
        "public_road": "Boolean or 0/1",
        "time_of_day": "morning, afternoon, evening",
        "holiday": "Boolean or 0/1",
        "school_season": "Boolean or 0/1",
        "num_reported_accidents": "Non-negative number"
    }

    audit = []

    for feature in FEATURES:
        value = segment.get(feature)

        if feature in missing:
            status = "Missing"
        elif feature in invalid:
            status = "Invalid"
        elif feature in imputed_fields:
            status = "ImputedFromTrainingData"
        else:
            status = "Valid"

        if feature in imputed_fields:
            source = "Original training dataset median/mode"

        elif feature in {"weather", "lighting"}:
            source = "Open-Meteo"

        elif feature == "time_of_day":
            source = "South African local arrival time"

        elif feature in {"holiday", "school_season"}:
            source = "South African calendar"

        elif feature == "road_type":
            source = segment.get(
                "_road_type_source",
                "No verified live road classification"
            )

        elif feature == "num_lanes":
            source = segment.get(
                "_lanes_source",
                "No verified live lane count"
            )

        elif feature == "curvature":
            source = (
                "Request or training fallback; OSM geometry proxy excluded"
            )

        elif feature == "public_road":
            source = segment.get(
                "_public_road_source",
                "No verified live access information"
            )

        elif feature == "speed_limit":
            source = "Training speed-limit units unverified"

        elif feature in original_segment:
            source = "C# request"

        else:
            source = "No verified live source"

        audit.append({
            "feature": feature,
            "expectedFormat": expected_formats.get(feature),
            "actualValue": value,
            "source": source,
            "status": status,
            "formatValid": status in {
                "Valid",
                "ImputedFromTrainingData"
            }
        })

    return audit


# =========================================================
# RISK CLASSIFICATION
# =========================================================

def _risk_level(score):
    index = int(np.digitize(score, CUTS))
    index = min(index, len(LEVELS) - 1)
    return LEVELS[index]


def _explanations(df, row_index, community):
    reasons = []

    try:
        contributions = MODEL.predict(
            df.iloc[[row_index]],
            pred_contrib=True
        )[0]

        labels = {
            "curvature": "Road curvature",
            "speed_limit": "Speed limit",
            "weather": "Weather conditions",
            "lighting": "Lighting conditions",
            "num_reported_accidents": "Previously reported accidents",
            "num_lanes": "Number of lanes",
            "road_type": "Road type",
            "road_signs_present": "Road signs",
            "public_road": "Public road indicator",
            "time_of_day": "Time of day",
            "holiday": "Holiday indicator",
            "school_season": "School-season indicator"
        }

        pairs = zip(FEATURES, contributions[:len(FEATURES)])

        for name, value in sorted(
            pairs,
            key=lambda item: abs(float(item[1])),
            reverse=True
        )[:5]:

            contribution = float(value)

            if abs(contribution) < 1e-6:
                continue

            reasons.append({
                "factor": labels.get(name, name),
                "direction": (
                    "increases" if contribution > 0 else "reduces"
                ),
                "contribution": round(contribution, 5),
                "method": "LightGBM contribution"
            })

    except Exception as exc:
        print(f"Model explanation unavailable: {exc}")

    for item in community.get("explanations", []):
        reasons.append({
            "factor": item.get("type", "Community report"),
            "direction": "increases",
            "method": "Approved community report",
            "severity": item.get("severity"),
            "distanceMetres": item.get("distanceMetres"),
            "ageHours": item.get("ageHours")
        })

    return reasons


# =========================================================
# MODEL PREDICTION
# =========================================================

def _predict_segments(segments):
    results = [None] * len(segments)

    valid_indices = []
    rows = []
    imputed_by_index = {}

    for index, original_segment in enumerate(segments):
        prepared, imputed_fields = _prepare_model_segment(
            original_segment
        )

        missing, invalid = _validate_segment(prepared)

        if missing:
            results[index] = {
                "riskScore": None,
                "riskLevel": None,
                "status": "InsufficientData",
                "missingFields": missing,
                "invalidFields": invalid,
                "imputedFields": imputed_fields,
                "warnings": [
                    "Required features are missing and no fallback is available."
                ],
                "explanations": []
            }
            continue

        if invalid:
            results[index] = {
                "riskScore": None,
                "riskLevel": None,
                "status": "InvalidInput",
                "missingFields": [],
                "invalidFields": invalid,
                "imputedFields": imputed_fields,
                "warnings": [
                    "Correct invalid features before prediction."
                ],
                "explanations": []
            }
            continue

        valid_indices.append(index)
        imputed_by_index[index] = imputed_fields

        rows.append({
            feature: prepared[feature]
            for feature in FEATURES
        })

    if not rows:
        return results

    df = pd.DataFrame(rows, columns=FEATURES)

    for column, categories in CATS.items():
        df[column] = pd.Categorical(
            df[column].astype(str).str.strip().str.lower(),
            categories=categories
        )

    for column in BOOLEAN_FEATURES:
        if column in df.columns:
            df[column] = df[column].map(
                lambda value: int(bool(_boolean_value(value)))
            ).astype("int8")

    model_scores = np.clip(
        MODEL.predict(df),
        0,
        1
    )

    for local_index, (original_index, model_score) in enumerate(
        zip(valid_indices, model_scores)
    ):
        segment = segments[original_index]

        community = score_approved_reports(
            segment.get("community_reports", [])
        )

        community_component = (
            community["score"]
            if community["approvedReportsUsed"]
            else None
        )

        final_score = float(np.clip(
            blend_risk(
                model_score,
                community_component,
                COMMUNITY_ALPHA
            ),
            0,
            1
        ))

        level = _risk_level(final_score)

        nearest_cut = min(
            CUTS,
            key=lambda cutoff: abs(final_score - cutoff)
        )

        borderline = abs(final_score - nearest_cut) < MARGIN

        possible_level = None

        if borderline:
            cut_index = CUTS.index(nearest_cut)

            possible_level = (
                LEVELS[cut_index + 1]
                if level == LEVELS[cut_index]
                else LEVELS[cut_index]
            )

        imputed_fields = imputed_by_index[original_index]

        warnings = []

        if imputed_fields:
            warnings.append(
                "Some model features use training-data estimates, not live values."
            )

        if "speed_limit" in imputed_fields:
            warnings.append(
                "Speed limit was estimated; training units are unverified."
            )

        if "curvature" in imputed_fields:
            warnings.append(
                "Curvature was estimated because compatible live geometry was unavailable."
            )

        if "num_reported_accidents" in imputed_fields:
            warnings.append(
                "Accident history was estimated because verified historical data was unavailable."
            )

        if "public_road" in imputed_fields:
            warnings.append(
                "Public-road status was estimated because verified access data was unavailable."
            )

        results[original_index] = {
            "riskScore": round(final_score, 4),
            "modelRiskScore": round(float(model_score), 4),
            "communityRiskScore": (
                float(community["score"])
                if community["approvedReportsUsed"]
                else None
            ),
            "communityReportsUsed": int(
                community["approvedReportsUsed"]
            ),
            "riskLevel": level,
            "status": "Valid",
            "isBorderline": bool(borderline),
            "alsoPossible": possible_level,
            "imputedFields": imputed_fields,
            "dataQuality": (
                "Estimated" if imputed_fields else "Complete"
            ),
            "warnings": warnings,
            "explanations": _explanations(
                df,
                local_index,
                community
            )
        }

    return results


# =========================================================
# HEALTH ENDPOINT
# =========================================================

@app.get("/health")
def health():
    return _json_safe({
        "status": "ok",
        "model": "LightGBM",
        "modelFile": os.path.basename(MODEL_PATH),
        "trainingFallbackFileAvailable": os.path.exists(
            TRAINING_DATA_PATH
        ),
        "trainingFallbackFeaturesAvailable": sorted(
            TRAINING_DEFAULTS.keys()
        ),
        "features": FEATURES,
        "categories": CATS,
        "thresholds": TH,
        "borderlineMargin": MARGIN,
        "communityBlendAlpha": COMMUNITY_ALPHA,
        "speedLimitContract": (
            "Training units unverified; live speed limits excluded."
        ),
        "curvatureContract": (
            "OSM geometry bend proxy excluded until training semantics are verified."
        ),
        "roadDataProvider": "NGDA Gauteng Roads Main Layer 21",
        "osmRoadDataProvider": "OpenStreetMap local South Africa extract",
        "osmRoadDatabaseAvailable": os.path.exists(
            os.path.join(BASE_DIR, "data", "osm_roads_gauteng.sqlite")
        ),
        "timeZone": "Africa/Johannesburg",
        "calendarSupport": {
            "publicHolidays": [2026],
            "schoolTerms": [2026]
        }
    })


# =========================================================
# PREPARED FEATURE PREDICTION
# =========================================================

@app.post("/predict")
def predict(batch: Batch):
    return _json_safe({
        "results": _predict_segments(batch.segments)
    })


# =========================================================
# LIVE ROUTE PREDICTION
# =========================================================

@app.post("/predict-route")
async def predict_route(batch: RouteBatch):
    points = []

    for index, segment in enumerate(batch.segments):
        try:
            latitude = float(segment["latitude"])
            longitude = float(segment["longitude"])

            if not (
                math.isfinite(latitude)
                and math.isfinite(longitude)
                and -90 <= latitude <= 90
                and -180 <= longitude <= 180
            ):
                raise ValueError("Coordinates outside WGS84 ranges.")

        except (KeyError, TypeError, ValueError):
            raise HTTPException(
                status_code=422,
                detail=(
                    f"segments[{index}] requires valid latitude and longitude."
                )
            )

        points.append({
            "latitude": latitude,
            "longitude": longitude,
            "arrival_time": segment.get("arrival_time")
        })

    # Weather retrieval
    try:
        weather_results = await fetch_route_weather(points)

        if len(weather_results) != len(points):
            raise RuntimeError(
                "Weather provider returned an unexpected result count."
            )

    except (
        httpx.HTTPError,
        ValueError,
        RuntimeError,
        TypeError
    ) as exc:
        raise HTTPException(
            status_code=502,
            detail=f"Weather retrieval failed: {exc}"
        ) from exc

    # NGDA road retrieval
    road_lookup_error = None

    try:
        road_results = await fetch_route_roads(points)

        if len(road_results) != len(points):
            raise RuntimeError(
                "Road provider returned an unexpected result count."
            )

        matched = sum(road is not None for road in road_results)

        road_lookup_status = (
            "ok" if matched == len(points)
            else "partial" if matched > 0
            else "no_match"
        )

    except (
        httpx.HTTPError,
        ValueError,
        RuntimeError,
        TypeError
    ) as exc:
        road_results = [None] * len(points)
        road_lookup_status = "unavailable"
        road_lookup_error = str(exc)

    # Feature enrichment
    prepared = []
    osm_results = []

    for index, (segment, weather, road) in enumerate(
        zip(batch.segments, weather_results, road_results)
    ):
        point = points[index]
        item = dict(segment)

        # Weather
        weather_features = _prepare_weather_features(weather)

        for feature, value in weather_features.items():
            if value is not None:
                item[feature] = value

        # South African time
        derived_time = _derive_time_of_day(segment)

        if derived_time is not None:
            item["time_of_day"] = derived_time

        # South African calendar
        for feature, value in _derive_calendar_features(segment).items():
            if value is not None:
                item[feature] = value

        # NGDA metadata and classification
        if road:
            for key in (
                "road_name",
                "source",
                "source_record_id",
                "gid",
                "town",
                "geometry",
                "distance_metres",
                "road_type_raw",
                "road_category_raw",
                "speed_limit_kmh"
            ):
                if road.get(key) is not None:
                    item[f"road_{key}"] = road[key]

        _apply_ngda_features(item, road)

        # Local OSM
        osm_road = _lookup_osm(
            point["latitude"],
            point["longitude"]
        )

        osm_results.append(osm_road)

        _apply_osm_features(item, osm_road)

        prepared.append(item)

    # Predictions
    predictions = _predict_segments(prepared)

    first_prediction = predictions[0] if predictions else None

    first_prepared, first_imputed = (
        _prepare_model_segment(prepared[0])
        if prepared else ({}, [])
    )

    first_road = dict(road_results[0] or {})

    if osm_results and osm_results[0]:
        osm = osm_results[0]

        first_road.update({
            "osm_road_type": osm.get("road_type"),
            "osm_num_lanes": _osm_lane_count(osm),
            "osm_speed_limit": osm.get("speed_limit"),
            "osm_geometry_curvature": osm.get("model_curvature"),
            "model_public_road": osm.get("model_public_road"),
            "access": osm.get("access"),
            "motor_vehicle": osm.get("motor_vehicle"),
            "osm_source_record_id": osm.get("source_record_id")
        })

    response = {
        "results": predictions,

        "requestReceivedFromCSharp": {
            "segments": batch.segments
        },

        "featureAudit": (
            _feature_audit(
                first_prepared,
                batch.segments[0],
                first_road,
                weather_results[0],
                first_imputed
            )
            if prepared else []
        ),

        "missingFeatureError": (
            first_prediction
            if first_prediction
            and first_prediction["status"] in {
                "InsufficientData",
                "InvalidInput"
            }
            else None
        ),

        "selectedSegmentRoadInformation": (
            road_results[0] if road_results else None
        ),

        "selectedSegmentOSMRoadInformation": (
            osm_results[0] if osm_results else None
        ),

        "osmRoadInformation": osm_results,
        "osmLookupProvider": "OpenStreetMap local South Africa extract",
        "osmRoadMatchesFound": sum(
            road is not None for road in osm_results
        ),

        "selectedSegmentWeather": (
            weather_results[0] if weather_results else None
        ),

        "weather": weather_results,
        "weatherProvider": "Open-Meteo",

        "roadInformation": road_results,
        "roadLookupProvider": "NGDA Gauteng Roads Main Layer 21",
        "roadLookupStatus": road_lookup_status,
        "roadLookupError": road_lookup_error,
        "roadMatchesFound": sum(
            road is not None for road in road_results
        ),

        "routePointsProcessed": len(points)
    }

    return _json_safe(response)
