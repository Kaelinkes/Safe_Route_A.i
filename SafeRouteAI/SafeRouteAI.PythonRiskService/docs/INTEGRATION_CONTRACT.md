# SafeRoute AI — ASP.NET Integration Contract

## Service details

Default local base URL: `http://127.0.0.1:8001`
OpenAPI UI: `/docs`
Health check: `GET /health`

## Required model fields per segment

| Field | Type / accepted values |
|---|---|
| `road_type` | `highway`, `rural`, `urban` |
| `num_lanes` | Positive integer |
| `curvature` | Non-negative number; expected range 0–1 |
| `speed_limit_kmh` | Non-negative number for `/predict-route`; converted under provisional legacy mph assumption |
| `lighting` | Set by weather service on `/predict-route`; for `/predict`, `daylight`, `dim`, `night` |
| `weather` | Set by weather service on `/predict-route`; for `/predict`, `clear`, `foggy`, `rainy` |
| `road_signs_present` | Boolean or 0/1 |
| `public_road` | Boolean or 0/1 |
| `time_of_day` | `morning`, `afternoon`, `evening` |
| `holiday` | Boolean or 0/1 |
| `school_season` | Boolean or 0/1 |
| `num_reported_accidents` | Non-negative number; define a historical look-back window to avoid future leakage |

The internal model field is named `speed_limit`. `/predict-route` converts the supplied `speed_limit_kmh` to the legacy model scale. Do not send both speed fields in the same segment.

## Route prediction request

`POST /predict-route`

```json
{
  "segments": [
    {
      "latitude": -26.2041,
      "longitude": 28.0473,
      "arrival_time": "2026-11-07T16:00:00+02:00",
      "road_type": "urban",
      "num_lanes": 2,
      "curvature": 0.2,
      "speed_limit_kmh": 60,
      "road_signs_present": true,
      "public_road": true,
      "time_of_day": "afternoon",
      "holiday": false,
      "school_season": true,
      "num_reported_accidents": 3,
      "community_reports": []
    }
  ]
}
```

This is a schema example, not a guarantee that these example road attributes are correct for a real location. ASP.NET must source each road attribute from a trusted data source. `weather` and `lighting` are populated by the API after a successful Open-Meteo lookup.

## Prepared prediction request

`POST /predict`

```json
{
  "segments": [
    {
      "road_type": "urban",
      "num_lanes": 2,
      "curvature": 0.2,
      "speed_limit": 37.3,
      "lighting": "daylight",
      "weather": "clear",
      "road_signs_present": true,
      "public_road": true,
      "time_of_day": "afternoon",
      "holiday": false,
      "school_season": true,
      "num_reported_accidents": 3,
      "community_reports": []
    }
  ]
}
```

For `/predict`, `speed_limit` must already be expressed in the model's legacy expected unit. Avoid using this endpoint unless the ASP.NET side performs the agreed conversion correctly.

## Response handling

A successful segment result may contain:

- `status`: `Valid`
- `riskScore`: final risk score from 0 to 1
- `modelRiskScore`: base ML score
- `communityRiskScore`: separate community overlay score, or null
- `communityReportsUsed`: number of accepted reports
- `riskLevel`: model risk band
- `isBorderline` and `alsoPossible`: uncertainty around a risk-band cutoff
- `warnings`: e.g. speed outside the model's training range
- `explanations`: model contributions and approved community-report contributions

Non-valid segment statuses:

- `InsufficientData`: required feature(s) missing; inspect `missingFields`.
- `InvalidInput`: invalid values; inspect `invalidFields`.

HTTP-level failures:

- `422`: malformed request, missing/invalid route coordinates, or invalid request shape.
- `502`: upstream weather retrieval failed. Do not silently replace missing weather with `clear`.

## Community report object

Each report should contain:

```json
{
  "approved": true,
  "severity": "high",
  "reported_at": "2026-11-07T15:30:00+02:00",
  "distance_m": 120,
  "type": "vehicle stopped on shoulder"
}
```

Accepted severity values: `low`, `medium`, `high`, `critical`. Unapproved, stale/invalid, un-timestamped, unknown-severity, or out-of-radius reports are ignored by the scoring function. The API expects the ASP.NET service to enforce who can approve reports.

## ASP.NET implementation guidance

- Use a typed HTTP client and DTOs rather than loosely typed dictionaries in production.
- Configure the Python base URL outside source code (environment/configuration/secret store).
- Set a timeout suitable for upstream weather calls; add cancellation and retry policy carefully.
- Do not retry validation errors (422) as if they were transient.
- Treat warnings and borderline predictions as user-visible uncertainty, not guaranteed risk.
- Do not interpret feature contributions as causal explanations.
