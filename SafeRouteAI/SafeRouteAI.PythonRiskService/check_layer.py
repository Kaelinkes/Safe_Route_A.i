
import asyncio
import json
import httpx

URL = (
    "https://dpmegis.dpme.gov.za/arcgis/rest/services/"
    "Geospatial_Data_Layers/MapServer/78"
)

async def main():
    async with httpx.AsyncClient(timeout=30) as client:
        response = await client.get(
            URL,
            params={"f": "json"}
        )
        data = response.json()

        print(json.dumps({
            "name": data.get("name"),
            "geometryType": data.get("geometryType"),
            "extent": data.get("extent"),
            "spatialReference": data.get("spatialReference"),
            "maxRecordCount": data.get("maxRecordCount"),
            "supportsStatistics": data.get(
                "advancedQueryCapabilities", {}
            ).get("supportsStatistics")
        }, indent=2))

        query = await client.get(
            URL + "/query",
            params={
                "f": "json",
                "where": "1=1",
                "returnCountOnly": "true"
            }
        )

        print("\nTOTAL ROAD RECORDS:")
        print(json.dumps(query.json(), indent=2))

if __name__ == "__main__":
    asyncio.run(main())
