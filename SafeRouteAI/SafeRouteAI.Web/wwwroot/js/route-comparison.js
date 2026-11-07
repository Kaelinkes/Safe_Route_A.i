(() => {
    document.addEventListener("DOMContentLoaded", () => {
        const form = document.getElementById("routeRequestForm");
        const results = document.getElementById("routeCards");
        const findButton = document.getElementById("findRoutes");
        const sort = document.getElementById("routeSort");
        const riskNotice = document.getElementById("riskPredictionNotice");
        if (!form || !results || !findButton || !sort) return;

        let requestController = null;
        let routes = [];
        let isRoutingMock = false;
        let isRiskMock = false;

        function showMessage(text, state = "info") {
            const paragraph = document.createElement("p");
            paragraph.className = `route-message ${state}`;
            paragraph.setAttribute("role", "status");
            paragraph.setAttribute("aria-live", "polite");
            paragraph.textContent = text;
            results.replaceChildren(paragraph);
        }

        function riskPresentation(level) {
            const states = {
                Low: { css: "low", icon: "✓" },
                Medium: { css: "medium", icon: "!" },
                High: { css: "high", icon: "!" },
                InsufficientData: { css: "unknown", icon: "?" },
            };
            return states[level] || states.InsufficientData;
        }

        function parseLineString(value) {
            if (typeof value !== "string" || value.length === 0) return null;

            try {
                const geometry = JSON.parse(value);
                if (geometry?.type !== "LineString" ||
                    !Array.isArray(geometry.coordinates) ||
                    geometry.coordinates.length < 2) {
                    return null;
                }

                const validCoordinates = geometry.coordinates.every((position) =>
                    Array.isArray(position) &&
                    position.length >= 2 &&
                    position.every(Number.isFinite) &&
                    position[0] >= -180 && position[0] <= 180 &&
                    position[1] >= -90 && position[1] <= 90,
                );

                return validCoordinates ? geometry : null;
            } catch {
                return null;
            }
        }

        function setMapStatus(message) {
            document.dispatchEvent(new CustomEvent("saferoute:route-map-status", {
                detail: { message },
            }));
        }

        function updateRiskNotice() {
            if (riskNotice) {
                riskNotice.hidden = !isRiskMock;
                riskNotice.textContent = isRiskMock
                    ? "Risk predictions are mocked or unavailable. Route results may still be real, but no risk classification or score is presented as a verified safety assessment."
                    : "";
            }

            const lowestRiskOption = sort.querySelector('option[value="lowest-risk"]');
            if (lowestRiskOption) lowestRiskOption.disabled = isRiskMock;
            if (isRiskMock && sort.value === "lowest-risk") sort.value = "recommended";
        }

        function orderedRoutes() {
            const ordered = [...routes];
            switch (sort.value) {
                case "fastest":
                    ordered.sort((a, b) => a.durationMinutes - b.durationMinutes);
                    break;
                case "lowest-risk": {
                    const order = { Low: 0, Medium: 1, High: 2, InsufficientData: 3 };
                    ordered.sort((a, b) => order[a.riskLevel] - order[b.riskLevel]);
                    break;
                }
                default:
                    break;
            }
            return ordered;
        }

        function renderRoutes() {
            updateRiskNotice();

            if (isRoutingMock) {
                showMessage("The route service returned simulated routes. Calculated route options are not shown.", "error");
                sort.disabled = true;
                document.dispatchEvent(new Event("saferoute:route-map-clear"));
                setMapStatus("Route geometry is not shown because the routing response is marked simulated.");
                return;
            }

            const ordered = orderedRoutes();
            if (ordered.length === 0) {
                showMessage("No route options were returned for these locations.");
                sort.disabled = true;
                document.dispatchEvent(new Event("saferoute:route-map-clear"));
                setMapStatus("No route geometry is available because the service returned no routes.");
                return;
            }

            const cards = ordered.map((route) => {
                const card = document.createElement("article");
                card.className = "route-result-card";

                const top = document.createElement("div");
                top.className = "route-card-top";
                const title = document.createElement("b");
                title.textContent = `Route ${route.routeId}`;
                const risk = isRiskMock
                    ? { css: "unknown", icon: "?" }
                    : riskPresentation(route.riskLevel);
                const badge = document.createElement("span");
                badge.className = `risk ${risk.css}`;
                const icon = document.createElement("span");
                icon.className = "risk-icon";
                icon.setAttribute("aria-hidden", "true");
                icon.textContent = risk.icon;
                const riskLabel = isRiskMock
                    ? "Risk unavailable"
                    : route.riskLevel === "InsufficientData"
                        ? "Insufficient data"
                        : route.riskLevel;
                badge.append(icon, document.createTextNode(` ${riskLabel}`));
                top.append(title, badge);

                const metrics = document.createElement("div");
                metrics.className = "route-metrics";
                const duration = document.createElement("strong");
                duration.textContent = `${route.durationMinutes} min`;
                const distance = document.createElement("span");
                distance.textContent = `${route.distanceKm} km`;
                metrics.append(duration, distance);

                const score = document.createElement("p");
                score.className = "route-score";
                const scoreLabel = document.createElement("b");
                scoreLabel.textContent = "Risk score: ";
                const scoreValue = isRiskMock
                    ? "Unavailable (mock prediction)"
                    : route.riskScore === null
                        ? "Not provided"
                        : String(route.riskScore);
                score.append(scoreLabel, document.createTextNode(scoreValue));

                const factor = document.createElement("p");
                factor.className = "route-factor";
                factor.textContent = "Contributing factors: not provided by the route API.";

                card.append(top, metrics, score, factor);
                return card;
            });

            results.replaceChildren(...cards);
            sort.disabled = false;

            const routesWithGeometry = ordered.map((route) => ({
                routeId: route.routeId,
                geometry: parseLineString(route.geometry),
            }));
            document.dispatchEvent(new CustomEvent("saferoute:route-geometries", {
                detail: {
                    routes: routesWithGeometry,
                    totalRoutes: ordered.length,
                },
            }));
        }

        async function requestRoutes(detail) {
            requestController?.abort();
            requestController = new AbortController();
            findButton.disabled = true;
            findButton.setAttribute("aria-busy", "true");
            const originalContent = findButton.innerHTML;
            findButton.textContent = "Finding routes…";
            sort.disabled = true;
            routes = [];
            isRoutingMock = false;
            isRiskMock = false;
            updateRiskNotice();
            document.dispatchEvent(new Event("saferoute:route-map-clear"));
            setMapStatus("Waiting for route calculation.");
            showMessage("Requesting route options…", "info");

            const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
            try {
                const response = await fetch(form.dataset.routeUrl, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        Accept: "application/json",
                        ...(token ? { RequestVerificationToken: token } : {}),
                    },
                    credentials: "same-origin",
                    signal: requestController.signal,
                    body: JSON.stringify({
                        origin: detail.origin,
                        destination: detail.destination,
                    }),
                });
                const body = await response.json().catch(() => null);

                if (response.status === 400) {
                    showMessage(body?.detail || "The route request was invalid. Check both selected locations and try again.", "error");
                    setMapStatus("Route geometry is unavailable because the route request was rejected.");
                    return;
                }
                if (response.status === 503) {
                    showMessage(body?.detail || "Route calculation is currently unavailable. Please try again later.", "error");
                    setMapStatus("Route geometry is unavailable because route calculation is unavailable.");
                    return;
                }
                if (!response.ok) {
                    showMessage("Route calculation could not be completed because the service returned an error.", "error");
                    setMapStatus("Route geometry is unavailable because route comparison failed.");
                    return;
                }

                if (!body || !Array.isArray(body.routes) ||
                    typeof body.isRoutingMock !== "boolean" ||
                    typeof body.isRiskMock !== "boolean") {
                    showMessage("The route service returned an incomplete response. No route results are shown.", "error");
                    setMapStatus("Route geometry is unavailable because the service response was incomplete.");
                    return;
                }

                routes = body.routes;
                isRoutingMock = body.isRoutingMock;
                isRiskMock = body.isRiskMock;
                renderRoutes();
            } catch (error) {
                if (error.name === "AbortError") return;
                showMessage("Route calculation could not reach the service. Please try again later.", "error");
                setMapStatus("Route geometry is unavailable because route calculation did not complete.");
            } finally {
                findButton.disabled = false;
                findButton.removeAttribute("aria-busy");
                findButton.innerHTML = originalContent;
            }
        }

        document.addEventListener("saferoute:route-requested", (event) => {
            requestRoutes(event.detail || {});
        });
        document.addEventListener("saferoute:routes-cleared", () => {
            requestController?.abort();
            routes = [];
            isRoutingMock = false;
            isRiskMock = false;
            updateRiskNotice();
            sort.disabled = true;
            document.dispatchEvent(new Event("saferoute:route-map-clear"));
            setMapStatus("Select origin and destination points to request route geometry.");
            showMessage("Select origin and destination points on the map to request route options.");
        });
        sort.addEventListener("change", renderRoutes);
    });
})();
