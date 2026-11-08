
"""Resolve a road coordinate to an NGI Geographic ID (GID)."""

import math
import os

import httpx


NGI_ROAD_LAYER = os.getenv(
    "NGI_ROAD_LAYER",
    "https://gis.westerncape.gov.za/server2/rest/services/"
    "SpatialDataWarehouse/Transportation/MapServer/2"
)

SEARCH_RADIUS_METRES = 250
MAX_MATCH_DISTANCE_METRES = 50

FIELDS = (
    "GID,CUID,FEAT_TYPE,ENTITY_NAME,ROAD_SURFACE,"
    "SOURCE_CURR,SOURCE_ACCU"
)


def _distance_to_segment_m(lat, lon, a, b):
    """Approximate point-to-segment distance in metres."""
    scale_x = 111320 * math.cos(math.radians(lat))
    scale_y = 110540

    ax = (a[0] - lon) * scale_x
    ay = (a[1] - lat) * scale_y
    bx = (b[0] - lon) * scale_x
    by = (b[1] - lat) * scale_y

    dx = bx - ax
    dy = by - ay
    length_sq = dx * dx + dy * dy

    if length_sq == 0:
        return math.hypot(ax, ay)

    t = max(0.0, min(1.0, -(ax * dx + ay * dy) / length_sq))
    return math.hypot(ax + t * dx, ay + t * dy)


def _geometry_distance(lat, lon, geometry):
    paths = geometry.get("paths", [])
    best = float("inf")

    for path in paths:
        if len(path) == 1:
            point = path[0]
            distance = _distance_to_segment_m(
                lat, lon, point, point
            )
            best = min(best, distance)
            continue

        for index in range(len(path) - 1):
            distance = _distance_to_segment_m(
                lat, lon, path[index], path[index + 1]
            )
            best = min(best, distance)

    return best


async def fetch_ngi_gid(
    latitude: float,
    longitude: float,
    client: httpx.AsyncClient | None = None,
):
    """
    Find the nearest NGI road and return its GID.

    Returns a diagnostic result. A missing GID is never fabricated.
    """

    if not -90 <= latitude <= 90:
        raise ValueError("Latitude must be between -90 and 90.")

    if not -180 <= longitude <= 180:
        raise ValueError("Longitude must be between -180 and 180.")

    params = {
        "f": "json",
        "where": "1=1",
        "geometry": f"{longitude},{latitude}",
        "geometryType": "esriGeometryPoint",
        "inSR": 4326,
        "spatialRel": "esriSpatialRelIntersects",
        "distance": SEARCH_RADIUS_METRES,
        "units": "esriSRUnit_Meter",
        "outFields": FIELDS,
        "returnGeometry": "true",
        "outSR": 4326,
    }

    owns_client = client is None
    http = client or httpx.AsyncClient(timeout=20.0)

    try:
        response = await http.get(
            f"{NGI_ROAD_LAYER}/query",
            params=params,
        )
        response.raise_for_status()
        payload = response.json()

        if "error" in payload:
            return {
                "status": "source_error",
                "source": "NGI",
                "error": payload["error"],
                "gid": None,
            }

        features = payload.get("features", [])

        if not features:
            return {
                "status": "no_match",
                "source": "NGI",
                "gid": None,
                "message": "No NGI road record found within search radius.",
                "candidates_found": 0,
            }

        candidates = []

        for feature in features:
            geometry = feature.get("geometry") or {}
            attributes = feature.get("attributes") or {}

            distance = _geometry_distance(
                latitude, longitude, geometry
            )

            candidates.append({
                "distance_metres": round(distance, 2),
                "attributes": attributes,
            })

        nearest = min(
            candidates,
            key=lambda item: item["distance_metres"],
        )

        attributes = nearest["attributes"]
        distance = nearest["distance_metres"]
        gid = attributes.get("GID")

        if distance > MAX_MATCH_DISTANCE_METRES:
            return {
                "status": "too_far",
                "source": "NGI",
                "gid": None,
                "nearest_distance_metres": distance,
                "candidates_found": len(candidates),
            }

        if gid is None:
            return {
                "status": "missing_gid",
                "source": "NGI",
                "gid": None,
                "nearest_distance_metres": distance,
                "attributes": attributes,
            }

        return {
            "status": "matched",
            "source": "NGI",
            "gid": gid,
            "cuid": attributes.get("CUID"),
            "road_type": attributes.get("FEAT_TYPE"),
            "road_name": attributes.get("ENTITY_NAME"),
            "road_surface": attributes.get("ROAD_SURFACE"),
            "source_date": attributes.get("SOURCE_CURR"),
            "source_accuracy": attributes.get("SOURCE_ACCU"),
            "distance_metres": distance,
            "candidates_found": len(candidates),
        }

    except httpx.HTTPError as exc:
        return {
            "status": "unavailable",
            "source": "NGI",
            "gid": None,
            "error": str(exc),
        }

    finally:
        if owns_client:
            await http.aclose()
