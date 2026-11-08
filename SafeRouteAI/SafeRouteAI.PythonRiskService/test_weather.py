import asyncio
import httpx
from weather_client import fetch_point_weather


def test_hourly_forecast_mapping_and_units():
    def handler(request):
        return httpx.Response(200, json={
            "current": {"time": "2026-11-06T10:00", "weather_code": 0, "is_day": 1},
            "hourly": {
                "time": ["2026-11-06T10:00", "2026-11-06T11:00"],
                "weather_code": [0, 61], "visibility": [20000, 5000],
                "precipitation": [0.0, 2.4], "is_day": [1, 1],
                "shortwave_radiation": [500, 100], "temperature_2m": [20, 18],
                "relative_humidity_2m": [40, 80], "wind_speed_10m": [10, 15],
                "wind_gusts_10m": [20, 25]
            }
        })
    async def run():
        async with httpx.AsyncClient(transport=httpx.MockTransport(handler)) as client:
            return await fetch_point_weather(-26.2, 28.0, "2026-11-06T11:00:00Z", client)
    result = asyncio.run(run())
    assert result["source"] == "hourly_forecast"
    assert result["weather"] == "rainy"
    assert result["lighting"] == "daylight"
    assert result["temperatureC"] == 18
    assert result["windSpeedKmh"] == 15
    assert result["precipitationMm"] == 2.4


def test_current_weather_used_near_now():
    def handler(request):
        return httpx.Response(200, json={
            "current": {"time": "2026-11-06T11:00", "weather_code": 45, "is_day": 0,
                        "temperature_2m": 17, "relative_humidity_2m": 90,
                        "precipitation": 0, "wind_speed_10m": 12, "wind_gusts_10m": 20},
            "hourly": {"time": ["2026-11-06T11:00"], "weather_code": [0]}
        })
    async def run():
        async with httpx.AsyncClient(transport=httpx.MockTransport(handler)) as client:
            return await fetch_point_weather(-26.2, 28.0, "2026-11-06T11:00:00Z", client)
    result = asyncio.run(run())
    assert result["source"] == "current"
    assert result["weather"] == "foggy"
    assert result["lighting"] == "night"
