
"""Gauteng road enrichment using the NGDA ArcGIS road network.

Confirmed source:
NGDA Gauteng MapServer layer 21 - Roads Main.

Important:
- OBJECTID is a source-local identifier, NOT a universal GID.
- Road geometry is retrieved from the ArcGIS service.
- Missing road features remain None.
- No model features are fabricated.
"""

from __future__ import annotations

import asyncio
import json
import math
import os
from typing import Any

import httpx


NGDA_SERVICE_URL = (
    "https://ndagis.nda.agric.za/arcgis/rest/services/"
    "Gauteng/MapServer"
)

ROAD_LAYER_ID = 21

SEARCH_RADIUS_METRES = float(
    os.getenv("NGDA_SEARCH_RADIUS_METRES", "1000")
)

MAX_MATCH_DISTANCE_METRES = float(
    os.getenv("NGDA_MAX_MATCH_DISTANCE_METRES", "50")
)

MAX_CONCURRENT_LOOKUPS = int(
    os.getenv("NGDA_MAX_CONCURRENT_LOOKUPS", "5")
)

REQUEST_TIMEOUT = float(
    os.getenv("NGDA_REQUEST_TIMEOUT", "30")
)

BASE_FIELDS = [
    "OBJECTID",
    "street",
    "town",
]


def _layer_url(layer_id: int = ROAD_LAYER_ID) -> str:
    return f"{NGDA_SERVICE_URL}/{layer_id}"


def _to_local_metres(
    longitude: float,
    latitude: float,
    reference_latitude: float,
) -> tuple[float, float]:
    """Convert coordinates to a local metre-based coordinate system."""

    earth_radius = 6371000.0

    x = (
        math.radians(longitude)
        * earth_radius
        * math.cos(math.radians(reference_latitude))
    )

    y = math.radians(latitude) * earth_radius

    return x, y


def _point_to_segment_distance(
    px: float,
    py: float,
    ax: float,
    ay: float,
    bx: float,
    by: float,
) -> float:
    """Calculate the shortest distance from a point to a line segment."""

    dx = bx - ax
    dy = by - ay

    length_squared = dx * dx + dy * dy

    if length_squared == 0:
        return math.hypot(px - ax, py - ay)

    t = (
        (px - ax) * dx + (py - ay) * dy
    ) / length_squared

    t = max(0.0, min(1.0, t))

    closest_x = ax + t * dx
    closest_y = ay + t * dy

    return math.hypot(
        px - closest_x,
        py - closest_y,
    )


def _geometry_distance(
    longitude: float,
    latitude: float,
    geometry: dict[str, Any],
) -> float:
    """Calculate the closest distance to an ArcGIS polyline in metres."""

    paths = geometry.get("paths", [])

    if not paths:
        return float("inf")

    px, py = _to_local_metres(
        longitude,
        latitude,
        latitude,
    )

    best_distance = float("inf")

    for path in paths:
        if not path:
            continue

        coordinates = [
            _to_local_metres(
                coordinate[0],
                coordinate[1],
                latitude,
            )
            for coordinate in path
        ]

        if len(coordinates) == 1:
            x, y = coordinates[0]

            best_distance = min(
                best_distance,
                math.hypot(px - x, py - y),
            )

            continue

        for index in range(len(coordinates) - 1):
            ax, ay = coordinates[index]
            bx, by = coordinates[index + 1]

            distance = _point_to_segment_distance(
                px,
                py,
                ax,
                ay,
                bx,
                by,
            )

            best_distance = min(
                best_distance,
                distance,
            )

    return best_distance


async def fetch_layer_fields(
    client: httpx.AsyncClient,
    layer_id: int = ROAD_LAYER_ID,
) -> set[str]:
    """Retrieve the actual fields exposed by the NGDA layer."""

    response = await client.get(
        _layer_url(layer_id),
        params={"f": "json"},
    )

    response.raise_for_status()

    metadata = response.json()

    if "error" in metadata:
        raise RuntimeError(
            f"NGDA layer metadata error: {metadata['error']}"
        )

    return {
        field["name"]
        for field in metadata.get("fields", [])
        if field.get("name")
    }


async def get_dpme_field_inventory() -> dict[str, list[str]]:
    """Preserve the existing field-inventory function name."""

    async with httpx.AsyncClient(
        timeout=REQUEST_TIMEOUT,
        follow_redirects=True,
    ) as client:

        fields = await fetch_layer_fields(client)

    return {
        "NGDA_GAUTENG_ROADS_MAIN": sorted(fields)
    }


def _normalise_road_record(
    attributes: dict[str, Any],
    geometry: dict[str, Any],
    distance_metres: float,
) -> dict[str, Any]:
    """Return a road record using the existing enrichment structure."""

    road_name = attributes.get("street")
    town = attributes.get("town")

    return {
        "source": "NGDA Gauteng",
        "source_layer_id": ROAD_LAYER_ID,

        # Source-local identifier only.
        "source_record_id": attributes.get("OBJECTID"),

        "road_id": None,
        "section_id": None,
        "segment_code": None,
        "identifier": None,

        # No universal GID has been confirmed.
        "gid": None,

        "road_name": road_name,
        "town": town,
        "road_type_raw": None,
        "road_category_raw": None,
        "speed_limit_kmh": None,
        "route_code": None,
        "num_lanes_raw": None,

        "road_signs_raw": None,
        "public_road_raw": None,
        "accident_data_raw": None,

        "pavement_type": None,
        "direction": None,
        "province": "Gauteng",

        # Actual geometry returned by NGDA.
        "geometry": geometry,

        "raw_attributes": attributes,
        "distance_metres": round(distance_metres, 2),
        "matched": True,
    }


async def _query_layer(
    client: httpx.AsyncClient,
    longitude: float,
    latitude: float,
) -> dict[str, Any] | None:
    """Query NGDA and select the closest matching road line."""

    params = {
        "f": "json",
        "where": "1=1",

        # JSON geometry is required for this working query pattern.
        "geometry": json.dumps({
            "x": longitude,
            "y": latitude,
            "spatialReference": {"wkid": 4326},
        }),

        "geometryType": "esriGeometryPoint",
        "inSR": 4326,
        "spatialRel": "esriSpatialRelIntersects",
        "distance": SEARCH_RADIUS_METRES,
        "units": "esriSRUnit_Meter",
        "outFields": ",".join(BASE_FIELDS),
        "returnGeometry": "true",
        "outSR": 4326,
        "returnZ": "false",
        "returnM": "false",
    }

    response = await client.get(
        f"{_layer_url()}/query",
        params=params,
    )

    response.raise_for_status()

    payload = response.json()

    if "error" in payload:
        raise RuntimeError(
            f"NGDA road query error: {payload['error']}"
        )

    features = payload.get("features", [])

    best_record = None
    best_distance = float("inf")

    for feature in features:
        attributes = feature.get("attributes") or {}
        geometry = feature.get("geometry") or {}

        if not geometry.get("paths"):
            continue

        distance = _geometry_distance(
            longitude,
            latitude,
            geometry,
        )

        if distance < best_distance:
            best_distance = distance

            best_record = {
                "attributes": attributes,
                "geometry": geometry,
            }

    if best_record is None:
        return None

    if best_distance > MAX_MATCH_DISTANCE_METRES:
        return None

    return _normalise_road_record(
        attributes=best_record["attributes"],
        geometry=best_record["geometry"],
        distance_metres=best_distance,
    )


async def fetch_nearest_road(
    latitude: float,
    longitude: float,
    client: httpx.AsyncClient | None = None,
) -> dict[str, Any] | None:
    """Find the nearest matching Gauteng road."""

    if not math.isfinite(latitude) or not -90 <= latitude <= 90:
        raise ValueError("Latitude must be between -90 and 90.")

    if not math.isfinite(longitude) or not -180 <= longitude <= 180:
        raise ValueError("Longitude must be between -180 and 180.")

    owns_client = client is None

    http = client or httpx.AsyncClient(
        timeout=REQUEST_TIMEOUT,
        follow_redirects=True,
    )

    try:
        return await _query_layer(
            http,
            longitude,
            latitude,
        )

    finally:
        if owns_client:
            await http.aclose()


async def fetch_route_roads(
    points: list[Any],
) -> list[dict[str, Any] | None]:
    """
    Retrieve the nearest road for each route point.

    Accepted formats:
      {"latitude": ..., "longitude": ...}
      {"lat": ..., "lon": ...}
      (latitude, longitude)
      [latitude, longitude]

    Coordinate pairs are interpreted as latitude, longitude.
    """

    semaphore = asyncio.Semaphore(
        MAX_CONCURRENT_LOOKUPS
    )

    async with httpx.AsyncClient(
        timeout=REQUEST_TIMEOUT,
        follow_redirects=True,
    ) as client:

        async def lookup(
            point: Any,
        ) -> dict[str, Any] | None:

            if isinstance(point, dict):
                latitude = point.get(
                    "latitude",
                    point.get("lat"),
                )

                longitude = point.get(
                    "longitude",
                    point.get("lon", point.get("lng")),
                )

            elif isinstance(point, (list, tuple)) and len(point) >= 2:
                latitude, longitude = point[0], point[1]

            else:
                raise ValueError(
                    f"Unsupported route point: {point!r}"
                )

            if latitude is None or longitude is None:
                raise ValueError(
                    f"Route point has missing coordinates: {point!r}"
                )

            async with semaphore:
                try:
                    return await fetch_nearest_road(
                        latitude=float(latitude),
                        longitude=float(longitude),
                        client=client,
                    )

                except (
                    httpx.HTTPError,
                    RuntimeError,
                    ValueError,
                ):
                    return None

        return await asyncio.gather(
            *(lookup(point) for point in points)
        )
