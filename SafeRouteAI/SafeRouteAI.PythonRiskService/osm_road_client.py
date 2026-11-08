
import json
import math
import os
import re
import sqlite3

import osmium


BASE_DIR = os.path.dirname(os.path.abspath(__file__))
PBF_PATH = os.path.join(BASE_DIR, "data", "south-africa-latest.osm.pbf")
DB_PATH = os.path.join(BASE_DIR, "data", "osm_roads_gauteng.sqlite")

MIN_LON, MAX_LON = 27.0, 29.5
MIN_LAT, MAX_LAT = -27.5, -25.0
SEARCH_RADIUS_METRES = 150


def distance_to_segment(lat, lon, a, b):
    lat_scale = 111320.0
    lon_scale = 111320.0 * math.cos(math.radians(lat))

    ax = (a[0] - lon) * lon_scale
    ay = (a[1] - lat) * lat_scale
    bx = (b[0] - lon) * lon_scale
    by = (b[1] - lat) * lat_scale

    dx, dy = bx - ax, by - ay
    length_sq = dx * dx + dy * dy

    if length_sq == 0:
        return math.hypot(ax, ay)

    t = max(
        0.0,
        min(1.0, -(ax * dx + ay * dy) / length_sq)
    )

    return math.hypot(ax + t * dx, ay + t * dy)


def classify_road(highway):
    highway = str(highway or "").lower().strip()

    if highway in {
        "motorway", "motorway_link",
        "trunk", "trunk_link",
        "primary", "primary_link"
    }:
        return "highway"

    if highway in {
        "residential", "living_street",
        "tertiary", "tertiary_link",
        "secondary", "secondary_link",
        "service", "road"
    }:
        return "urban"

    if highway in {"track", "unclassified"}:
        return "rural"

    return None


def _parse_positive_integer(value):
    if value is None:
        return None

    match = re.fullmatch(r"\s*(\d+)\s*", str(value))

    if not match:
        return None

    number = int(match.group(1))
    return number if 1 <= number <= 12 else None


def parse_lanes(value, forward=None, backward=None):
    lanes = _parse_positive_integer(value)

    if lanes is not None:
        return lanes

    forward_lanes = _parse_positive_integer(forward)
    backward_lanes = _parse_positive_integer(backward)

    if forward_lanes is not None and backward_lanes is not None:
        total = forward_lanes + backward_lanes
        return total if total <= 12 else None

    if forward_lanes is not None:
        return forward_lanes

    if backward_lanes is not None:
        return backward_lanes

    return None


def parse_speed_limit(value):
    if value is None:
        return None

    raw = str(value).strip().lower()

    match = re.fullmatch(
        r"(\d+(?:\.\d+)?)\s*(km/?h|kph)",
        raw
    )

    if not match:
        return None

    speed = float(match.group(1))
    return speed if 10 <= speed <= 160 else None


def calculate_geometry_curvature(geometry):
    """
    Geometry-derived bend index, not verified engineering curvature.
    """

    if not geometry or len(geometry) < 3:
        return None

    bearings = []

    for i in range(len(geometry) - 1):
        lon1, lat1 = geometry[i]
        lon2, lat2 = geometry[i + 1]

        if (lon1, lat1) == (lon2, lat2):
            continue

        lat1_rad = math.radians(lat1)
        lat2_rad = math.radians(lat2)
        delta_lon = math.radians(lon2 - lon1)

        y = math.sin(delta_lon) * math.cos(lat2_rad)

        x = (
            math.cos(lat1_rad) * math.sin(lat2_rad)
            - math.sin(lat1_rad) * math.cos(lat2_rad)
            * math.cos(delta_lon)
        )

        bearings.append(math.degrees(math.atan2(y, x)))

    if len(bearings) < 2:
        return None

    total_turn = 0.0

    for previous, current in zip(bearings, bearings[1:]):
        difference = abs(current - previous)
        total_turn += min(difference, 360.0 - difference)

    return round(min(1.0, total_turn / 180.0), 4)


def classify_public_road(access, motor_vehicle=None):
    access = str(access or "").lower().strip()
    motor_vehicle = str(motor_vehicle or "").lower().strip()

    if access in {"private", "no"}:
        return False

    if motor_vehicle in {"private", "no"}:
        return False

    if access in {"yes", "permissive"}:
        return True

    if motor_vehicle in {"yes", "permissive"}:
        return True

    return None


def _row_value(row, available_columns, column, default=None):
    if column not in available_columns:
        return default

    return row[column]


def create_database():
    """
    Build a new database only.
    Never overwrite an existing database.
    """

    if os.path.exists(DB_PATH):
        raise FileExistsError(
            "Existing OSM database found. It was not modified."
        )

    if not os.path.exists(PBF_PATH):
        raise FileNotFoundError(
            f"OSM extract not found: {PBF_PATH}"
        )

    os.makedirs(os.path.dirname(DB_PATH), exist_ok=True)

    connection = sqlite3.connect(DB_PATH)

    connection.execute("""
        CREATE TABLE roads (
            osm_id INTEGER PRIMARY KEY,
            name TEXT,
            highway TEXT,
            lanes TEXT,
            lanes_forward TEXT,
            lanes_backward TEXT,
            maxspeed TEXT,
            access TEXT,
            motor_vehicle TEXT,
            surface TEXT,
            lit TEXT,
            oneway TEXT,
            geometry TEXT,
            min_lon REAL,
            max_lon REAL,
            min_lat REAL,
            max_lat REAL
        )
    """)

    class RoadHandler(osmium.SimpleHandler):

        def __init__(self):
            super().__init__()
            self.count = 0

        def way(self, way):
            highway = way.tags.get("highway")

            if not highway:
                return

            try:
                coordinates = [
                    [node.lon, node.lat]
                    for node in way.nodes
                    if node.location.valid()
                ]

                if len(coordinates) < 2:
                    return

                lons = [point[0] for point in coordinates]
                lats = [point[1] for point in coordinates]

                bounds = (
                    min(lons), max(lons),
                    min(lats), max(lats)
                )

                if (
                    bounds[1] < MIN_LON
                    or bounds[0] > MAX_LON
                    or bounds[3] < MIN_LAT
                    or bounds[2] > MAX_LAT
                ):
                    return

                tags = way.tags

                connection.execute("""
                    INSERT OR REPLACE INTO roads VALUES (
                        ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?,
                        ?, ?, ?, ?
                    )
                """, (
                    way.id,
                    tags.get("name"),
                    highway,
                    tags.get("lanes"),
                    tags.get("lanes:forward"),
                    tags.get("lanes:backward"),
                    tags.get("maxspeed"),
                    tags.get("access"),
                    tags.get("motor_vehicle"),
                    tags.get("surface"),
                    tags.get("lit"),
                    tags.get("oneway"),
                    json.dumps(coordinates),
                    *bounds
                ))

                self.count += 1

                if self.count % 10000 == 0:
                    connection.commit()
                    print(f"Indexed roads: {self.count}")

            except Exception as error:
                print(f"Skipping OSM way {way.id}: {error}")

    try:
        print("Building Gauteng OSM database...")

        handler = RoadHandler()
        handler.apply_file(PBF_PATH, locations=True)

        connection.commit()

        connection.execute("""
            CREATE INDEX idx_road_bounds
            ON roads(min_lon, max_lon, min_lat, max_lat)
        """)

        connection.commit()

        print(f"Roads indexed: {handler.count}")

    finally:
        connection.close()


def find_nearest_road(
    latitude,
    longitude,
    radius_metres=SEARCH_RADIUS_METRES
):
    if not os.path.exists(DB_PATH):
        raise FileNotFoundError(
            f"OSM database missing: {DB_PATH}"
        )

    connection = sqlite3.connect(DB_PATH)
    connection.row_factory = sqlite3.Row

    try:
        # Read the actual database schema.
        available_columns = {
            column["name"]
            for column in connection.execute(
                "PRAGMA table_info(roads)"
            ).fetchall()
        }

        required_columns = {
            "osm_id", "highway", "geometry",
            "min_lon", "max_lon", "min_lat", "max_lat"
        }

        missing = required_columns - available_columns

        if missing:
            raise RuntimeError(
                f"OSM database is missing required columns: {sorted(missing)}"
            )

        lat_margin = radius_metres / 111320.0

        lon_margin = radius_metres / (
            111320.0 * max(
                0.1,
                math.cos(math.radians(latitude))
            )
        )

        rows = connection.execute("""
            SELECT *
            FROM roads
            WHERE max_lon >= ?
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
                  'living_street', 'service', 'road',
                  'track'
              )
        """, (
            longitude - lon_margin,
            longitude + lon_margin,
            latitude - lat_margin,
            latitude + lat_margin
        )).fetchall()

    finally:
        connection.close()

    best = None

    for row in rows:
        try:
            geometry = json.loads(row["geometry"])

            if not isinstance(geometry, list) or len(geometry) < 2:
                continue

            distances = [
                distance_to_segment(
                    latitude,
                    longitude,
                    geometry[i],
                    geometry[i + 1]
                )
                for i in range(len(geometry) - 1)
            ]

            distance = min(distances)

            if distance > radius_metres:
                continue

            if best is not None and distance >= best["distance_metres"]:
                continue

            highway = row["highway"]

            lanes_raw = _row_value(
                row, available_columns, "lanes"
            )

            lanes_forward = _row_value(
                row, available_columns, "lanes_forward"
            )

            lanes_backward = _row_value(
                row, available_columns, "lanes_backward"
            )

            maxspeed = _row_value(
                row, available_columns, "maxspeed"
            )

            access = _row_value(
                row, available_columns, "access"
            )

            motor_vehicle = _row_value(
                row, available_columns, "motor_vehicle"
            )

            surface = _row_value(
                row, available_columns, "surface"
            )

            lighting = _row_value(
                row, available_columns, "lit"
            )

            oneway = _row_value(
                row, available_columns, "oneway"
            )

            road_name = _row_value(
                row, available_columns, "name"
            )

            curvature = calculate_geometry_curvature(geometry)

            best = {
                "source": "OpenStreetMap",
                "source_record_id": row["osm_id"],
                "gid": None,
                "road_name": road_name,

                "road_type": highway,
                "num_lanes": lanes_raw,
                "lanes_forward": lanes_forward,
                "lanes_backward": lanes_backward,
                "speed_limit": maxspeed,
                "access": access,
                "motor_vehicle": motor_vehicle,
                "surface": surface,
                "lighting_tag": lighting,
                "oneway": oneway,

                "model_road_type": classify_road(highway),
                "model_num_lanes": parse_lanes(
                    lanes_raw,
                    lanes_forward,
                    lanes_backward
                ),
                "model_speed_limit_kmh": parse_speed_limit(
                    maxspeed
                ),
                "model_public_road": classify_public_road(
                    access,
                    motor_vehicle
                ),
                "model_curvature": curvature,

                "curvature_source": (
                    "OpenStreetMap geometry"
                    if curvature is not None
                    else None
                ),

                "geometry": geometry,
                "distance_metres": round(distance, 2),
                "matched": True
            }

        except (ValueError, TypeError, KeyError, json.JSONDecodeError):
            continue

    return best


if __name__ == "__main__":
    result = find_nearest_road(-26.107032, 28.056842)
    print(json.dumps(result, indent=2))
