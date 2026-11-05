# Sample database seed data

This document describes the sample rows from the DBeaver T-SQL seed script
shared in the project conversation. It is a record of the intended dataset,
not a confirmation that every row was inserted into a particular database.
The script must be executed against `SafeRouteDomainTest` for these rows to
appear there.

The sample uses generated identity keys. Relationships below are described by
their natural identifiers and values, not fixed numeric IDs; the actual IDs
depend on the database's contents when the script runs.

## Users

| Name | Email | Role | Registered (UTC) | Password hash |
|---|---|---|---|---|
| Seed Route User | `seed-route-user@example.test` | User | 2026-01-15 09:00 | `SEED_DATA_NOT_A_LOGIN_HASH` |
| Seed Reviewer | `seed-reviewer@example.test` | Admin | 2026-01-15 09:05 | `SEED_DATA_NOT_A_LOGIN_HASH` |

The password values are placeholders, not password hashes. These sample
accounts must not be used for authentication.

## Routes

Both routes belong to Seed Route User.

| Origin | Destination | Requested (UTC) |
|---|---|---|
| Braamfontein | Rosebank | 2026-01-16 08:30 |
| Rosebank | Sandton | 2026-01-16 17:15 |

## Road segments

| Name | Type | Start (latitude, longitude) | End (latitude, longitude) |
|---|---|---|---|
| Sample Road A | Urban | (-26.204100, 28.047300) | (-26.205000, 28.050000) |
| Sample Road B | Urban | (-26.205000, 28.050000) | (-26.207000, 28.053000) |
| Sample Road C | Urban | (-26.207000, 28.053000) | (-26.210000, 28.057000) |

## Weather observations

| Location (latitude, longitude) | Observed (UTC) | Precipitation | Humidity | Visibility | Wind speed |
|---|---|---:|---:|---:|---:|
| (-26.205000, 28.050000) | 2026-01-16 08:00 | 0.0 | 45.0 | 10.0 | 12.5 |
| (-26.207000, 28.053000) | 2026-01-16 08:00 | 0.2 | 52.0 | 8.0 | 18.0 |

The Domain model does not attach units to these numeric weather values; the
values are illustrative sample data.

## Risk assessments and explanations

| Road segment | Weather location | Risk score | Risk level | Assessment status | Explanation |
|---|---|---:|---|---|---|
| Sample Road A | (-26.205000, 28.050000) | 0.18 | Low | Seed assessment A | Sample low-risk assessment for test data. |
| Sample Road B | (-26.207000, 28.053000) | 0.46 | Medium | Seed assessment B | Sample moderate-risk assessment for test data. |

Each assessment has one associated `RiskExplanation`.

## Route options

Both options belong to the Braamfontein-to-Rosebank route.

| Rank | Route info | Geometry value | Distance | Estimated minutes | Risk score | Road segments, in order |
|---:|---|---|---:|---:|---:|---|
| 1 | Sample route via Road A and B | `_p~iF~ps\|U_ulLnnqC_mqNvxq\`@` | 4.2 | 14.0 | 0.28 | Sample Road A, Sample Road B |
| 2 | Alternative sample route | `_p~iF~ps\|U_ulLnnqC_mqNvxq\`@` | 4.8 | 17.0 | 0.19 | Sample Road A, Sample Road B, Sample Road C |

The geometry strings are illustrative encoded-polyline text; they are not
validated by the database or guaranteed to represent the sample road
coordinates.

## Community reports

Both reports were submitted by Seed Route User.

| Location (latitude, longitude) | Incident | Description | Reported (UTC) | Status | Reviewer | Reviewed (UTC) |
|---|---|---|---|---|---|---|
| (-26.205000, 28.050000) | Hazard | Sample road hazard report. | 2026-01-16 08:10 | Reviewed | Seed Reviewer | 2026-01-16 08:20 |
| (-26.207000, 28.053000) | Lighting | Sample report of poor lighting. | 2026-01-16 18:30 | Pending | Seed Reviewer | NULL |

The second row's reviewer is assigned by the shared script even though its
status is Pending and `reviewed_at` is NULL. This is simply what the seed
statement specifies; the database does not enforce status semantics.

## Route risk logs

Both log entries refer to the Braamfontein-to-Rosebank route.

| Logged (UTC) | Result snapshot |
|---|---|
| 2026-01-16 08:35 | `{"source":"seed","riskScore":0.28,"riskLevel":"Low"}` |
| 2026-01-16 17:20 | `{"source":"seed","riskScore":0.19,"riskLevel":"Low"}` |

## Accident records

| Location (latitude, longitude) | Occurred (UTC) | Accident type | Severity |
|---|---|---|---|
| (-26.205000, 28.050000) | 2026-01-10 14:00 | Collision | Minor |
| (-26.207000, 28.053000) | 2026-01-12 19:30 | Pedestrian incident | Moderate |
| (-26.210000, 28.057000) | 2026-01-14 07:45 | Collision | Minor |

## Model version metadata

These are metadata rows only; the seed script does not create model files.

| Version label | Training date (UTC) | Dataset version | Model path | Validation summary |
|---|---|---|---|---|
| seed-v1 | 2026-01-01 00:00 | sample-dataset-1 | `/models/seed-v1` | Sample seed metadata only. |
| seed-v2 | 2026-02-01 00:00 | sample-dataset-2 | `/models/seed-v2` | Sample seed metadata only. |

## Intended row counts

When run against an empty database with the expected schema, the script was
intended to add the following 29 rows:

| Table | Rows |
|---|---:|
| `users` | 2 |
| `routes` | 2 |
| `road_segments` | 3 |
| `weather_logs` | 2 |
| `risk_assessments` | 2 |
| `risk_explanations` | 2 |
| `route_options` | 2 |
| `route_option_road_segments` | 5 |
| `community_reports` | 2 |
| `route_risk_logs` | 2 |
| `accident_records` | 3 |
| `model_versions` | 2 |
| **Total** | **29** |

These are expected counts for the described dataset, not guaranteed insert
counts. The script uses existence checks to avoid adding matching rows again;
if a prerequisite row is missing, dependent inserts can insert zero rows.
DBeaver's affected-row message may report only the currently executed
statement. Check each table's row count to verify the database contents.

The sample data is illustrative. It is not production data and does not
represent validated risk predictions, real incidents, actual route provider
output, authentication credentials, or deployable machine-learning models.
