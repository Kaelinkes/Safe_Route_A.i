
from pathlib import Path

root = Path(__file__).resolve().parent
osm_file = root / "osm_road_client.py"
api_file = root / "serve_risk.py"

# -------------------------------------------------
# 1. Exclude non-driving OSM features from lookup
# -------------------------------------------------

osm = osm_file.read_text(encoding="utf-8-sig")

old = '''        WHERE max_lon >= ?
          AND min_lon <= ?
          AND max_lat >= ?
          AND min_lat <= ?
'''

new = '''        WHERE max_lon >= ?
          AND min_lon <= ?
          AND max_lat >= ?
          AND min_lat <= ?
          AND highway IN (
              'motorway', 'motorway_link',
              'trunk', 'trunk_link',
              'primary', 'primary_link',
              'secondary', 'secondary_link',
              'tertiary', 'tertiary_link',
              'unclassified', 'residential',
              'living_street', 'service', 'road'
          )
'''

if old not in osm:
    raise RuntimeError("Could not locate OSM road lookup SQL.")

osm = osm.replace(old, new, 1)
osm_file.write_text(osm, encoding="utf-8")

# -------------------------------------------------
# 2. Integrate OSM into the prediction API
# -------------------------------------------------

api = api_file.read_text(encoding="utf-8-sig")

def replace_once(source, old_text, new_text, label):
    if old_text not in source:
        raise RuntimeError(f"Could not locate API section: {label}")
    return source.replace(old_text, new_text, 1)


api = replace_once(
    api,
    "from dpme_road_client import fetch_route_roads\n",
    "from dpme_road_client import fetch_route_roads\n"
    "from osm_road_client import find_nearest_road\n",
    "OSM import"
)

# OSM lane parsing and source preparation
anchor = '''# ---------------------------------------------------------
# FEATURE VALIDATION
# ---------------------------------------------------------
'''

addition = '''# ---------------------------------------------------------
# LOCAL OSM FEATURE ENRICHMENT
# ---------------------------------------------------------

def _osm_lane_count(road):
    if not road:
        return None

    value = road.get("num_lanes")

    if value is None:
        return None

    try:
        number = float(str(value).strip())

        if number >= 1 and number.is_integer():
            return int(number)

    except (TypeError, ValueError):
        pass

    return None


def _lookup_osm(latitude, longitude):
    try:
        return find_nearest_road(
            latitude,
            longitude,
            radius_metres=150
        )
    except (OSError, ValueError, RuntimeError):
        return None


'''

api = replace_once(api, anchor, addition + anchor, "OSM helpers")

# Feature audit source attribution
old = '''        elif feature in original_seg:
            source = "C# request"

        elif feature == "road_type":
            source = "NGDA Gauteng; classification unavailable"
'''

new = '''        elif (
            feature == "num_lanes"
            and road is not None
            and road.get("osm_num_lanes") is not None
        ):
            source = "OpenStreetMap local South Africa extract"

        elif (
            feature == "road_type"
            and road is not None
            and road.get("osm_road_type")
        ):
            source = "OpenStreetMap; raw classification; model mapping unavailable"

        elif feature in original_seg:
            source = "C# request"

        elif feature == "road_type":
            source = "NGDA Gauteng / OpenStreetMap; no valid model category"
'''

api = replace_once(api, old, new, "feature audit sources")

old = '''        if feature == "road_type" and road is not None:
            item["rawSourceValue"] = {
                "road_type_raw": road.get("road_type_raw"),
                "road_category_raw": road.get("road_category_raw")
            }
'''

new = '''        if feature == "road_type" and road is not None:
            item["rawSourceValue"] = {
                "road_type_raw": road.get("road_type_raw"),
                "road_category_raw": road.get("road_category_raw"),
                "osm_road_type": road.get("osm_road_type")
            }

        if feature == "num_lanes" and road is not None:
            item["rawSourceValue"] = {
                "osm_num_lanes": road.get("osm_num_lanes")
            }
'''

api = replace_once(api, old, new, "feature audit raw values")

# Health metadata
old = '''        "roadDataProvider": "NGDA Gauteng Roads Main (Layer 21)",
        "sharedGidAvailable": False,
'''

new = '''        "roadDataProvider": "NGDA Gauteng Roads Main (Layer 21)",
        "osmRoadDataProvider": "OpenStreetMap local South Africa extract",
        "osmRoadDatabaseAvailable": os.path.exists(
            os.path.join(
                os.path.dirname(os.path.abspath(__file__)),
                "data",
                "osm_roads_gauteng.sqlite"
            )
        ),
        "sharedGidAvailable": False,
'''

api = replace_once(api, old, new, "health metadata")

# Store OSM and combined road results
old = '''    # Prepare model features.
    for seg, weather, road in zip(
        batch.segments,
        weather_results,
        road_results
    ):
        item = dict(seg)
'''

new = '''    # Prepare model features using NGDA and local OSM.
    osm_results = []
    combined_roads = []

    for index, (seg, weather, road) in enumerate(zip(
        batch.segments,
        weather_results,
        road_results
    )):
        point = points[index]

        osm_road = _lookup_osm(
            point["latitude"],
            point["longitude"]
        )

        osm_results.append(osm_road)

        combined_road = dict(road or {})

        if osm_road is not None:
            combined_road["osm_source"] = osm_road.get("source")
            combined_road["osm_source_record_id"] = osm_road.get(
                "source_record_id"
            )
            combined_road["osm_road_name"] = osm_road.get("road_name")
            combined_road["osm_road_type"] = osm_road.get("road_type")
            combined_road["osm_num_lanes"] = _osm_lane_count(osm_road)
            combined_road["osm_speed_limit"] = osm_road.get("speed_limit")
            combined_road["osm_distance_metres"] = osm_road.get(
                "distance_metres"
            )

        combined_roads.append(combined_road or None)

        item = dict(seg)
'''

api = replace_once(api, old, new, "route enrichment loop")

# Add OSM values to prepared model features
old = '''            # Missing road attributes remain missing.
            # No unsupported speed conversion is performed.

        prepared.append(item)
'''

new = '''            # Missing road attributes remain missing.
            # No unsupported speed conversion is performed.

        # Local OSM road classification and lane enrichment.
        if osm_road is not None:
            item["osm_road_name"] = osm_road.get("road_name")
            item["osm_road_type"] = osm_road.get("road_type")
            item["osm_source_record_id"] = osm_road.get("source_record_id")
            item["osm_distance_metres"] = osm_road.get("distance_metres")

            # Only use an OSM classification if it exactly matches
            # a category accepted by the trained model.
            osm_type = osm_road.get("road_type")

            if osm_type is not None:
                normalized = str(osm_type).strip().lower()

                if normalized in CATS.get("road_type", []):
                    item["road_type"] = normalized

            # Only accept a clearly numeric lane count.
            osm_lanes = _osm_lane_count(osm_road)

            if osm_lanes is not None:
                item["num_lanes"] = osm_lanes

        prepared.append(item)
'''

api = replace_once(api, old, new, "OSM model feature mapping")

# Use combined source information in feature audit
api = replace_once(
    api,
    "_feature_audit(\n"
    "                prepared[0],\n"
    "                batch.segments[0],\n"
    "                road_results[0],\n"
    "                weather_results[0]\n"
    "            )",
    "_feature_audit(\n"
    "                prepared[0],\n"
    "                batch.segments[0],\n"
    "                combined_roads[0],\n"
    "                weather_results[0]\n"
    "            )",
    "combined feature audit"
)

# Return OSM information in the API response
old = '''        "selectedSegmentRoadInformation": (
            road_results[0] if road_results else None
        ),

        "selectedSegmentWeather": (
'''

new = '''        "selectedSegmentRoadInformation": (
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
'''

api = replace_once(api, old, new, "OSM response fields")

api_file.write_text(api, encoding="utf-8")

print("OSM integration applied to serve_risk.py.")
print("OSM lookup now excludes non-driving road classes.")
print("Existing NGDA and weather integrations preserved.")
