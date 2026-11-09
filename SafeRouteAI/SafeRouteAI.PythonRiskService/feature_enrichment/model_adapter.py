import joblib
import pandas as pd
from pathlib import Path

BASE = Path(__file__).resolve().parent
MODEL_PATH = BASE.parent / "saferoute_s5e10_model.joblib"

bundle = joblib.load(MODEL_PATH)
model = bundle["model"]
FEATURES = bundle["features"]
CATEGORIES = bundle["categories"]
THRESHOLDS = bundle["thresholds"]

def predict_risk(features):
    missing = [f for f in FEATURES if f not in features or features[f] is None]
    if missing:
        return {
            "status": "InsufficientData",
            "missingFields": missing,
            "riskScore": None,
            "riskLevel": None
        }

    row = {}

    for feature in FEATURES:
        value = features[feature]

        if feature in CATEGORIES:
            allowed = CATEGORIES[feature]
            if value not in allowed:
                raise ValueError(
                    f"Invalid {feature}: {value}. Allowed: {allowed}"
                )
            row[feature] = pd.Categorical(
                [value], categories=allowed
            )
        else:
            row[feature] = [value]

    if not model.pandas_categorical:
        raise RuntimeError(
            "The model has no pandas categorical metadata. "
            "Training-time categorical encoding must be verified "
            "before making predictions."
        )

    expected_categories = model.pandas_categorical
    if len(expected_categories) != len(CATEGORIES):
        raise RuntimeError(
            "Categorical metadata count does not match bundle categories."
        )

    for expected, feature in zip(expected_categories, CATEGORIES):
        if list(expected) != list(CATEGORIES[feature]):
            raise RuntimeError(
                f"Category order mismatch for {feature}. "
                "Prediction stopped to prevent incorrect results."
            )

    frame = pd.DataFrame(row, columns=FEATURES)

    prediction = model.predict(frame)

    if getattr(prediction, "ndim", 1) != 1 or len(prediction) != 1:
        raise RuntimeError(
            "Unexpected prediction output. Expected one risk score."
        )

    score = float(prediction[0])

    if not 0 <= score <= 1:
        raise RuntimeError(
            f"Prediction is not a probability: {score}"
        )

    if score <= THRESHOLDS["low_max"]:
        level = "Low"
    elif score <= THRESHOLDS["medium_max"]:
        level = "Medium"
    else:
        level = "High"

    return {
        "status": "Success",
        "riskScore": round(score, 4),
        "riskLevel": level,
        "modelFeatures": len(FEATURES),
        "modelVersion": "saferoute_s5e10_model"
    }
