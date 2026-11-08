from datetime import datetime
from zoneinfo import ZoneInfo
import math

MODEL_FEATURES = [
    "road_type",
    "num_lanes",
    "curvature",
    "speed_limit",
    "lighting",
    "weather",
    "road_signs_present",
    "public_road",
    "time_of_day",
    "holiday",
    "school_season",
    "num_reported_accidents"
]

CATEGORIES = {
    "road_type": {"highway", "rural", "urban"},
    "lighting": {"daylight", "dim", "night"},
    "weather": {"clear", "foggy", "rainy"},
    "time_of_day": {"morning", "afternoon", "evening"}
}

BOOLEAN_FIELDS = {
    "road_signs_present",
    "public_road",
    "holiday",
    "school_season"
}

def derive_time_of_day(timestamp=None):
    if timestamp:
        dt = datetime.fromisoformat(timestamp.replace("Z", "+00:00"))
        if dt.tzinfo is None:
            dt = dt.replace(tzinfo=ZoneInfo("Africa/Johannesburg"))
        dt = dt.astimezone(ZoneInfo("Africa/Johannesburg"))
    else:
        dt = datetime.now(ZoneInfo("Africa/Johannesburg"))

    if 5 <= dt.hour < 12:
        return "morning"
    if 12 <= dt.hour < 17:
        return "afternoon"
    return "evening"


def validate_features(raw_features, sources=None):
    sources = sources or {}
    validated = {}
    audit = []

    for name in MODEL_FEATURES:
        value = raw_features.get(name)
        status = "valid"
        reason = None

        if value is None:
            status = "missing"
            reason = "No verified value supplied."

        elif name in CATEGORIES:
            if not isinstance(value, str) or value.strip().lower() not in CATEGORIES[name]:
                status = "invalid"
                reason = "Value does not match the model's accepted categories."
            else:
                value = value.strip().lower()

        elif name in BOOLEAN_FIELDS:
            if value in (0, 1) and not isinstance(value, bool):
                value = bool(value)
            elif not isinstance(value, bool):
                status = "invalid"
                reason = "Expected true, false, 0 or 1."

        elif name == "num_lanes":
            if (isinstance(value, bool) or
                not isinstance(value, (int, float)) or
                not math.isfinite(value) or
                int(value) != value or value <= 0):
                status = "invalid"
                reason = "Expected a positive whole number."

        elif name == "curvature":
            if (isinstance(value, bool) or
                not isinstance(value, (int, float)) or
                not math.isfinite(value) or
                not 0 <= value <= 1):
                status = "invalid"
                reason = "Expected a verified curvature value between 0 and 1."

        elif name in ("speed_limit", "num_reported_accidents"):
            if (isinstance(value, bool) or
                not isinstance(value, (int, float)) or
                not math.isfinite(value) or value < 0):
                status = "invalid"
                reason = "Expected a non-negative numeric value."

        if status != "valid":
            value = None

        validated[name] = value
        audit.append({
            "feature": name,
            "value": value,
            "source": sources.get(name),
            "status": status,
            "reason": reason
        })

    missing = [
        item["feature"]
        for item in audit
        if item["status"] != "valid"
    ]

    return {
        "features": validated,
        "availableFeatures": [
            item["feature"] for item in audit
            if item["status"] == "valid"
        ],
        "missingFeatures": missing,
        "featureAudit": audit,
        "predictionReady": len(missing) == 0
    }
