"""Map Open-Meteo weather observations to the categorical features used by SafeRoute AI.

The current Kaggle-trained bundle appears to have been trained with speed_limit in
mph (25-70); this assumption must be confirmed against the original CSV metadata.
We convert South African km/h to mph for this legacy bundle WITHOUT clipping. Values
outside the training range are flagged as extrapolations by the prediction API.
"""
from __future__ import annotations

FOG = {45, 48}
# WMO codes for drizzle/rain/showers/thunderstorms and snow. The legacy model has
# only clear/foggy/rainy categories, so all precipitation is mapped to rainy.
PRECIPITATION = set(range(51, 68)) | set(range(71, 78)) | set(range(80, 87)) | {95, 96, 99}


def map_weather(weather_code, visibility_m=None, precipitation_mm=None):
    if weather_code is None:
        return None
    try:
        code = int(weather_code)
    except (TypeError, ValueError):
        return None
    if code in FOG or (visibility_m is not None and float(visibility_m) < 1000):
        return "foggy"
    if code in PRECIPITATION or (precipitation_mm is not None and float(precipitation_mm) >= 0.1):
        return "rainy"
    return "clear"


def map_lighting(is_day, shortwave_radiation=None, dim_below_wm2=50):
    if is_day is None:
        return None
    try:
        if int(is_day) == 0:
            return "night"
        if shortwave_radiation is not None and float(shortwave_radiation) < dim_below_wm2:
            return "dim"
        return "daylight"
    except (TypeError, ValueError):
        return None


def kmh_to_model_speed(kmh):
    """Convert km/h to the legacy model's assumed mph input; never clip values.

    NOTE: 120/130 km/h become approximately 74.6/80.8 mph, outside the
    documented 25-70 range. The model can technically score them, but its result
    is extrapolative and must be flagged until retrained with verified units/range.
    """
    value = float(kmh)
    if value < 0:
        raise ValueError("speed limit cannot be negative")
    return value / 1.609344


def speed_range_warning(model_speed_mph, training_min=25.0, training_max=70.0):
    value = float(model_speed_mph)
    if value < training_min or value > training_max:
        return f"Converted speed limit {value:.1f} mph is outside the legacy model training range ({training_min:g}-{training_max:g} mph)."
    return None
