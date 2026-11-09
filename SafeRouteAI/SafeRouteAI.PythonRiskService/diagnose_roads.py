
import asyncio
import json
import httpx

URL = "https://ndagis.nda.agric.za/arcgis/rest/services/Gauteng/MapServer/21"

async def main():
    params = {
        "f": "json",
        "where": "1=1",
        "geometry": json.dumps({
            "x": 28.0567,
            "y": -26.1076,
            "spatialReference": {"wkid": 4326}
        }),
        "geometryType": "esriGeometryPoint",
        "inSR": 4326,
        "spatialRel": "esriSpatialRelIntersects",
        "distance": 1000,
        "units": "esriSRUnit_Meter",
        "outFields": "OBJECTID,street,town",
        "returnGeometry": "true",
        "outSR": 4326
    }

    async with httpx.AsyncClient(timeout=60) as client:
        response = await client.get(URL + "/query", params=params)
        print("HTTP:", response.status_code)
        data = response.json()

    print("FEATURES:", len(data.get("features", [])))

    for feature in data.get("features", [])[:5]:
        print(json.dumps(feature, indent=2))

if __name__ == "__main__":
    asyncio.run(main())
