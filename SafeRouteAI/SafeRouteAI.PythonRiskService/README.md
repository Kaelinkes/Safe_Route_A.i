# SafeRoute AI — Integration Package

**Package status:** Integration candidate / development release
**Prepared:** 7 November 2026
**Purpose:** Provide the Python risk-prediction service, trained model bundle, weather integration, community-report overlay, tests, and an integration contract for the SafeRoute AI application.

> **Important version note:** This folder is assembled from the latest complete SafeRouteAI model ZIP available in the project files. It contains the legacy LightGBM production bundle and its API contract. The separate `kmh_experiment` source files and model discussed in the 7 November development session were not present in that ZIP, so they are deliberately not recreated or substituted here. Do not mistake this folder for a byte-for-byte copy of the newest local Windows working directory. Keep a backup before merging it into the main project.

## 1. Folder contents

| File | Purpose |
|---|---|
| `serve_risk.py` | FastAPI service exposing `/health`, `/predict`, and `/predict-route` |
| `saferoute_s5e10_model.joblib` | Trained LightGBM model bundle used by the API |
| `weather_client.py` | Retrieves current/hourly route weather from Open-Meteo |
| `weather_mapper.py` | Maps weather/lighting and converts km/h to the legacy model speed unit assumption |
| `community_risk.py` | Scores approved community hazard reports and blends the overlay with ML risk |
| `requirements.txt` | Python dependencies |
| `train_model.py` | Optional model retraining script; not needed to run the existing API |
| `test_service.py`, `test_weather.py` | Automated API and weather tests |
| `metrics.json` | Baseline model evaluation metrics associated with the supplied bundle |
| `docs/PHASE5_IMPLEMENTATION_NOTES.md` | Implementation details, known limitations, and readiness notes |
| `docs/INTEGRATION_CONTRACT.md` | ASP.NET-to-Python integration contract and examples |
| `docs/SA_VALIDATION_STATUS.md` | South African data analysis findings and validation limitations |

## 2. Requirements

- Python 3.10+ recommended (use the same Python minor version as the environment in which the model is deployed, where possible).
- Network access from the Python service to `https://api.open-meteo.com` for `/predict-route`.
- The ASP.NET application must be able to reach the FastAPI service over an approved internal network route.

## 3. Install on Windows

Open Command Prompt or PowerShell in this folder:

```powershell
py -m venv .venv
.venv\Scripts\Activate.ps1
python -m pip install --upgrade pip
pip install -r requirements.txt
```

If PowerShell blocks activation, use Command Prompt instead:

```bat
.venv\Scripts\activate.bat
```

## 4. Configure and start the API

The API loads `saferoute_s5e10_model.joblib` from the current working directory by default. Start the service from this folder:

```powershell
uvicorn serve_risk:app --host 127.0.0.1 --port 8001
```

For access from another machine/container, bind to the appropriate interface only after configuring firewall rules and service authentication. Do not expose this development API directly to the public internet.

Check the service:

```powershell
Invoke-RestMethod http://127.0.0.1:8001/health
```

Interactive API documentation is available at `http://127.0.0.1:8001/docs` while the service is running.

## 5. API endpoints

- `GET /health` — reports service status, model feature names, categorical values, risk thresholds, speed contract, and configuration.
- `POST /predict` — accepts pre-prepared model features. No weather lookup is performed.
- `POST /predict-route` — accepts route-segment coordinates and model features, retrieves weather from Open-Meteo, maps weather/lighting, then predicts.

See [`docs/INTEGRATION_CONTRACT.md`](docs/INTEGRATION_CONTRACT.md) for the required feature schema and request/response examples.

## 6. Model feature contract

The supplied model expects exactly these 12 features:

`road_type`, `num_lanes`, `curvature`, `speed_limit`, `lighting`, `weather`, `road_signs_present`, `public_road`, `time_of_day`, `holiday`, `school_season`, `num_reported_accidents`.

Categorical values in this bundle:

- `road_type`: `highway`, `rural`, `urban`
- `lighting`: `daylight`, `dim`, `night`
- `weather`: `clear`, `foggy`, `rainy`
- `time_of_day`: `afternoon`, `evening`, `morning`

The model's `speed_limit` feature is in the legacy training scale, provisionally assumed to be mph. `/predict-route` accepts `speed_limit_kmh` and converts it to that scale. The original source unit has not been authoritatively verified. Values converted outside the bundle's training range of 25–70 are warned about and must be treated as extrapolations, not reliable South African predictions.

## 7. Run tests

With the virtual environment active:

```powershell
python -m pytest -q test_service.py test_weather.py
```

The packaged test files were run during assembly and passed **13 tests**. Re-run them in the target deployment environment after installing dependencies and before integration.

## 8. Community reports

Only approved reports with a recognised severity, timestamp, and distance within the configured 500 m radius contribute to the overlay. The default blend alpha (0.15), severity weights, and 72-hour half-life are provisional engineering settings. They require product-owner approval and calibration against local labelled outcomes before production use.

## 9. Integration checklist

1. Install and run the Python API in a controlled environment.
2. Call `/health` from the ASP.NET host and confirm the expected 12 feature names.
3. Map one ASP.NET route segment to one API segment; use WGS84 latitude/longitude for `/predict-route`.
4. Supply all required non-weather features; do not fabricate missing values.
5. Send `speed_limit_kmh` only after the project team accepts the documented provisional unit conversion, or use the separately maintained km/h experiment once its actual source/model files are merged and verified.
6. Map `riskScore`, `riskLevel`, `isBorderline`, `alsoPossible`, `warnings`, and `explanations` to the application UI.
7. Handle `InsufficientData`, `InvalidInput`, HTTP 422, and weather retrieval HTTP 502 explicitly.
8. Test the full ASP.NET → FastAPI → Open-Meteo → FastAPI → ASP.NET flow in a non-production environment.
9. Add service-to-service authentication, HTTPS, rate limits, timeouts/retries, caching, structured logs, monitoring, and Open-Meteo attribution/licence review before production.
