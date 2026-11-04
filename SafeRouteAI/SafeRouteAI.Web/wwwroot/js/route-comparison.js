(() => {
    document.addEventListener("DOMContentLoaded", () => {
        const form = document.getElementById("routeRequestForm");
        const results = document.getElementById("routeCards");
        const findButton = document.getElementById("findRoutes");
        const sort = document.getElementById("routeSort");
        if (!form || !results || !findButton || !sort) return;

        let requestController = null;
        let routes = [];
        let isMock = false;

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
            if (isMock) {
                showMessage("The route service returned mock data. These are not verified calculated routes, so route options are not shown.", "error");
                sort.disabled = true;
                return;
            }

            const ordered = orderedRoutes();
            if (ordered.length === 0) {
                showMessage("No route options were returned for these locations.");
                sort.disabled = true;
                return;
            }

            const cards = ordered.map((route) => {
                const card = document.createElement("article");
                card.className = "route-result-card";

                const top = document.createElement("div");
                top.className = "route-card-top";
                const title = document.createElement("b");
                title.textContent = `Route ${route.routeId}`;
                const risk = riskPresentation(route.riskLevel);
                const badge = document.createElement("span");
                badge.className = `risk ${risk.css}`;
                const icon = document.createElement("span");
                icon.className = "risk-icon";
                icon.setAttribute("aria-hidden", "true");
                icon.textContent = risk.icon;
                badge.append(icon, document.createTextNode(` ${route.riskLevel === "InsufficientData" ? "Insufficient data" : route.riskLevel}`));
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
                score.append(scoreLabel, document.createTextNode(route.riskScore === null ? "Not provided" : String(route.riskScore)));

                const factor = document.createElement("p");
                factor.className = "route-factor";
                factor.textContent = "Contributing factors: not provided by the route API.";

                card.append(top, metrics, score, factor);
                return card;
            });

            results.replaceChildren(...cards);
            sort.disabled = false;
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
            isMock = false;
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
                    return;
                }
                if (response.status === 503) {
                    showMessage(body?.detail || "Route calculation is currently unavailable. Please try again later.", "error");
                    return;
                }
                if (!response.ok) {
                    showMessage("Route calculation could not be completed because the service returned an error.", "error");
                    return;
                }

                if (!body || !Array.isArray(body.routes) || typeof body.isMock !== "boolean") {
                    showMessage("The route service returned an incomplete response. No route results are shown.", "error");
                    return;
                }

                routes = body.routes;
                isMock = body.isMock;
                renderRoutes();
            } catch (error) {
                if (error.name === "AbortError") return;
                showMessage("Route calculation could not reach the service. Please try again later.", "error");
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
            isMock = false;
            sort.disabled = true;
            showMessage("Select origin and destination points on the map to request route options.");
        });
        sort.addEventListener("change", renderRoutes);
    });
})();
