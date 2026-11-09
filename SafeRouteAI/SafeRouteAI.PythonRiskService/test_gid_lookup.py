
import asyncio
import json

from gid_enrichment import fetch_ngi_gid


async def main():
    # Sandton, Johannesburg
    result = await fetch_ngi_gid(
        latitude=-26.1076,
        longitude=28.0567,
    )

    print(json.dumps(result, indent=2, default=str))


if __name__ == "__main__":
    asyncio.run(main())
