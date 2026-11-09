import osmium
import math

PBF = "data/south-africa-latest.osm.pbf"

TARGET_LAT = -26.1076
TARGET_LON = 28.0567
SEARCH_RADIUS_M = 1000

def distance_to_segment_m(lat, lon, a, b):
    lat_scale = 111320
    lon_scale = 111320 * math.cos(math.radians(lat))

    px, py = 0.0, 0.0
    ax = (a.lon - lon) * lon_scale
    ay = (a.lat - lat) * lat_scale
    bx = (b.lon - lon) * lon_scale
    by = (b.lat - lat) * lat_scale

    dx, dy = bx - ax, by - ay
    length_sq = dx * dx + dy * dy

    if length_sq == 0:
        return math.hypot(ax, ay)

    t = max(0, min(1, -(ax * dx + ay * dy) / length_sq))
    return math.hypot(ax + t * dx, ay + t * dy)

class RoadHandler(osmium.SimpleHandler):
    def __init__(self):
        super().__init__()
        self.roads = []

    def way(self, w):
        if "highway" not in w.tags:
            return

        try:
            nodes = list(w.nodes)
            if len(nodes) < 2:
                return

            locations = [
                n.location for n in nodes
                if n.location.valid()
            ]

            if len(locations) < 2:
                return

            nearest = min(
                distance_to_segment_m(
                    TARGET_LAT, TARGET_LON,
                    locations[i], locations[i + 1]
                )
                for i in range(len(locations) - 1)
            )

            if nearest <= SEARCH_RADIUS_M:
                self.roads.append({
                    "distance_m": round(nearest, 2),
                    "osm_id": w.id,
                    "name": w.tags.get("name"),
                    "highway": w.tags.get("highway"),
                    "lanes": w.tags.get("lanes"),
                    "maxspeed": w.tags.get("maxspeed"),
                    "access": w.tags.get("access"),
                    "surface": w.tags.get("surface"),
                    "lit": w.tags.get("lit"),
                    "oneway": w.tags.get("oneway")
                })
        except Exception:
            pass

handler = RoadHandler()

print("Reading South Africa OSM extract...")
handler.apply_file(PBF, locations=True)

handler.roads.sort(key=lambda r: r["distance_m"])

print(f"\nRoads found within {SEARCH_RADIUS_M} metres: {len(handler.roads)}")

for road in handler.roads[:20]:
    print("-" * 55)
    for key, value in road.items():
        print(f"{key}: {value}")
