(() => {
    document.addEventListener("DOMContentLoaded", () => {
        const panel = document.querySelector(".weather-panel");
        const status = document.getElementById("plannerWeatherStatus");
        const readings = document.getElementById("plannerWeatherReadings");

        if (!panel || !status || !readings) return;

        let activeRequest = null;
        let requestNumber = 0;

        function showState(message, state = "info") {
            readings.hidden = true;
            status.textContent = message;
            status.className = `weather-status ${state}`;
        }

        function validCoordinates(latitude, longitude) {
            return Number.isFinite(latitude) && latitude >= -90 && latitude <= 90 &&
                Number.isFinite(longitude) && longitude >= -180 && longitude <= 180;
        }

        function formatValue(value, suffix, digits = 1) {
            return `${Number(value).toFixed(digits)} ${suffix}`;
        }

        function renderWeather(weather) {
            const values = {
                temperature: formatValue(weather.temperatureCelsius, "°C"),
                humidity: formatValue(weather.humidityPercent, "%"),
                precipitation: formatValue(weather.precipitationMm, "mm"),
                visibility: formatValue(weather.visibilityMeters, "m", 0),
                wind: formatValue(weather.windSpeedKmh, "km/h"),
                code: String(weather.weatherCode),
                observed: weather.observationTimeUtc,
            };

            Object.entries(values).forEach(([key, value]) => {
                const target = readings.querySelector(`[data-weather-value="${key}"]`);
                if (target) target.textContent = value;
            });

            status.textContent = `Weather information for ${weather.latitude.toFixed(4)}, ${weather.longitude.toFixed(4)}.`;
            status.className = "weather-status success";
            readings.hidden = false;
        }

        async function loadWeather(latitude, longitude) {
            if (!validCoordinates(latitude, longitude)) {
                showState("The selected map coordinates are invalid.", "error");
                return;
            }

            activeRequest?.abort();
            activeRequest = new AbortController();
            const currentRequest = ++requestNumber;
            showState("Loading weather information…", "loading");

            const url = new URL(panel.dataset.weatherUrl, window.location.origin);
            url.searchParams.set("latitude", String(latitude));
            url.searchParams.set("longitude", String(longitude));

            try {
                const response = await fetch(url, {
                    method: "GET",
                    headers: { Accept: "application/json" },
                    signal: activeRequest.signal,
                    credentials: "same-origin",
                });

                const body = await response.json().catch(() => null);
                if (currentRequest !== requestNumber) return;

                if (!response.ok) {
                    const detail = body?.detail || body?.title;
                    if (response.status === 400) {
                        showState(detail || "The weather service rejected these coordinates.", "error");
                    } else if (response.status === 503) {
                        showState("Weather information is currently unavailable. Please try again later.", "unavailable");
                    } else {
                        showState("Weather information could not be loaded because the service returned an error.", "error");
                    }
                    return;
                }

                const weather = body;
                const numericValues = [
                    weather?.latitude,
                    weather?.longitude,
                    weather?.temperatureCelsius,
                    weather?.humidityPercent,
                    weather?.precipitationMm,
                    weather?.visibilityMeters,
                    weather?.windSpeedKmh,
                    weather?.weatherCode,
                ];
                if (weather?.isAvailable !== true ||
                    !numericValues.every(Number.isFinite) ||
                    typeof weather.observationTimeUtc !== "string" ||
                    weather.observationTimeUtc.length === 0) {
                    showState("Weather information is unavailable or the service response was incomplete.", "unavailable");
                    return;
                }

                renderWeather(weather);
            } catch (error) {
                if (error.name === "AbortError") return;
                if (currentRequest !== requestNumber) return;
                showState("Weather information could not be reached. Please try again later.", "error");
            }
        }

        document.addEventListener("saferoute:weather-selected", (event) => {
            const { latitude, longitude } = event.detail || {};
            loadWeather(latitude, longitude);
        });

        document.addEventListener("saferoute:weather-cleared", () => {
            activeRequest?.abort();
            requestNumber++;
            showState("Select a point on the map to request weather information.");
        });
    });
})();
