
"""Open-Meteo live/current and hourly forecast client for route coordinates."""
from __future__ import annotations

from datetime import datetime, timezone
import asyncio
import httpx

from weather_mapper import map_weather, map_lighting

OPEN_METEO_URL = "https://api.open-meteo.com/v1/forecast"
HOURLY_FIELDS = (
    "weather_code,visibility,precipitation,is_day,"
    "shortwave_radiation,temperature_2m,relative_humidity_2m,"
    "wind_speed_10m,wind_gusts_10m"
)
CURRENT_FIELDS = (
    "weather_code,precipitation,is_day,temperature_2m,"
    "relative_humidity_2m,wind_speed_10m,wind_gusts_10m"
)


def _utc(value=None):
    if value is None:
        return datetime.now(timezone.utc)

    dt = (
        value
        if isinstance(value, datetime)
        else datetime.fromisoformat(
            str(value).replace("Z", "+00:00")
        )
    )

    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)

    return dt.astimezone(timezone.utc)


def _nearest_hourly(payload, target):
    hourly = payload.get("hourly") or {}
    times = hourly.get("time") or []

    if not times:
        return None

    target_naive = target.replace(tzinfo=None)
    parsed = [datetime.fromisoformat(t) for t in times]

    idx = min(
        range(len(parsed)),
        key=lambda i: abs(
            (parsed[i] - target_naive).total_seconds()
        )
    )

    return {
        key: (
            values[idx]
            if isinstance(values, list) and idx < len(values)
            else None
        )
        for key, values in hourly.items()
        if key != "time"
    }


async def fetch_point_weather(
    latitude,
    longitude,
    arrival_time=None,
    client=None
):
    lat, lon = float(latitude), float(longitude)

    if not -90 <= lat <= 90 or not -180 <= lon <= 180:
        raise ValueError(
            "latitude/longitude outside valid WGS84 ranges"
        )

    target = _utc(arrival_time)

    params = {
        "latitude": lat,
        "longitude": lon,
        "current": CURRENT_FIELDS,
        "hourly": HOURLY_FIELDS,
        "forecast_days": 7,
        "temperature_unit": "celsius",
        "wind_speed_unit": "kmh",
        "precipitation_unit": "mm",
        "timezone": "UTC"
    }

    own_client = client is None

    if own_client:
        client = httpx.AsyncClient(
            timeout=httpx.Timeout(12.0, connect=5.0)
        )

    try:
        response = await client.get(
            OPEN_METEO_URL,
            params=params
        )
        response.raise_for_status()
        payload = response.json()

    finally:
        if own_client:
            await client.aclose()

    current = payload.get("current") or {}
    current_time = (
        _utc(current.get("time"))
        if current.get("time")
        else None
    )

    if (
        current_time
        and abs((target - current_time).total_seconds())
        <= 30 * 60
    ):
        values = dict(current)
        source = "current"
        values.setdefault("visibility", None)
        values.setdefault("shortwave_radiation", None)

    else:
        values = _nearest_hourly(payload, target)
        source = "hourly_forecast"

    if values is None:
        raise RuntimeError(
            "Open-Meteo returned no usable current or hourly weather data"
        )

    weather = map_weather(
        values.get("weather_code"),
        values.get("visibility"),
        values.get("precipitation")
    )

    lighting = map_lighting(
        values.get("is_day"),
        values.get("shortwave_radiation")
    )

    return {
        "weather": weather,
        "lighting": lighting,
        "source": source,
        "forecastTime": target.isoformat(),
        "weatherCode": values.get("weather_code"),
        "temperatureC": values.get("temperature_2m"),
        "relativeHumidityPercent": values.get(
            "relative_humidity_2m"
        ),
        "precipitationMm": values.get("precipitation"),
        "visibilityMetres": values.get("visibility"),
        "windSpeedKmh": values.get("wind_speed_10m"),
        "windGustsKmh": values.get("wind_gusts_10m"),
        "isDay": values.get("is_day"),
        "shortwaveRadiationWm2": values.get(
            "shortwave_radiation"
        ),
        "units": {
            "temperature": "°C",
            "precipitation": "mm",
            "visibility": "m",
            "windSpeed": "km/h",
            "radiation": "W/m²"
        }
    }


async def fetch_route_weather(points):
    """Fetch route weather safely in batches of five coordinates."""

    results = []

    async with httpx.AsyncClient(
        timeout=httpx.Timeout(12.0, connect=5.0)
    ) as client:

        for start in range(0, len(points), 5):
            batch = points[start:start + 5]

            tasks = [
                fetch_point_weather(
                    point["latitude"],
                    point["longitude"],
                    point.get("arrival_time"),
                    client
                )
                for point in batch
            ]

            batch_results = await asyncio.gather(*tasks)
            results.extend(batch_results)

            # Small pause between batches.
            if start + 5 < len(points):
                await asyncio.sleep(0.5)

    return results
