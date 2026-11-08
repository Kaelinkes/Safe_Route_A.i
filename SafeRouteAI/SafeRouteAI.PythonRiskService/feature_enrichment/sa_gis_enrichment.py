import os
import requests

TIMEOUT = 15

GIS_SOURCES = {
    "CSIR": os.getenv("CSIR_ROADS_LAYER_URL", ""),
    "DPME": os.getenv("DPME_ROADS_LAYER_URL", "")
}


def query_layer(name, layer_url, latitude, longitude, radius=100):
    if not layer_url:
        return {
            "source": name,
            "status": "not_configured",
            "matches": []
        }

    params = {
        "f": "json",
        "geometry": f"{longitude},{latitude}",
        "geometryType": "esriGeometryPoint",
        "inSR": 4326,
        "spatialRel": "esriSpatialRelIntersects",
        "distance": radius,
        "units": "esriSRUnit_Meter",
        "outFields": "*",
        "returnGeometry": "true",
        "outSR": 4326
    }

    try:
        response = requests.get(
            layer_url.rstrip("/") + "/query",
            params=params,
            timeout=TIMEOUT
        )
        response.raise_for_status()
        data = response.json()

        if "error" in data:
            return {
                "source": name,
                "status": "source_error",
                "error": data["error"],
                "matches": []
            }

        matches = []

        for feature in data.get("features", []):
            matches.append({
                "attributes": feature.get("attributes", {}),
                "geometry": feature.get("geometry"),
                "source": name,
                "mappingStatus": "raw_unverified"
            })

        return {
            "source": name,
            "status": "ok" if matches else "no_match",
            "matches": matches
        }

    except requests.RequestException as error:
        return {
            "source": name,
            "status": "unavailable",
            "error": str(error),
            "matches": []
        }

    except ValueError as error:
        return {
            "source": name,
            "status": "invalid_response",
            "error": str(error),
            "matches": []
        }


def enrich_coordinate(latitude, longitude, radius=100):
    if not (-35 <= latitude <= -22 and 16 <= longitude <= 33):
        return {
            "status": "outside_south_africa",
            "results": []
        }

    results = []

    for name, url in GIS_SOURCES.items():
        results.append(
            query_layer(name, url, latitude, longitude, radius)
        )

    total_matches = sum(
        len(result["matches"]) for result in results
    )

    return {
        "status": "ok" if total_matches else "no_match",
        "totalMatches": total_matches,
        "results": results
    }
