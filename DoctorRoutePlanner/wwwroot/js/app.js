/*Variable declarations*/
let selectedUnit = "mi"; // default

/* Function definitions */
function formatTime(isoString) {
    if (!isoString) return "N/A";
    const date = new Date(isoString);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

let lastRoute = null;

function updateRouteSummary(route) {
    const distanceMeters = route.summary.totalDistance;
    const durationSeconds = route.summary.totalTime;

    const distance = selectedUnit === "mi"
        ? (distanceMeters / 1609.344).toFixed(2)
        : (distanceMeters / 1000).toFixed(2);

    const durationMin = Math.round(durationSeconds / 60);

    document.getElementById("distance").textContent = `${distance} ${selectedUnit}`;
    document.getElementById("eta").textContent = `${durationMin} min`;
}

/* EventListener to load the map and route data upon submission */
document.addEventListener("DOMContentLoaded", () => {
    const map = L.map("map").setView([40.4426, -79.9961], 12);

    // Define home waypoint
    const homeLocation = {
        name: "AHN Downtown Medical Center",
        latitude: 40.4426,
        longitude: -79.9961
    };
    const hospitalIcon = L.icon({
        iconUrl: "/images/icons8-doctors-bag-94.png", // 🏥 Hospital icon
        iconSize: [32, 32],
        iconAnchor: [16, 32],
        popupAnchor: [0, -32]
    });

    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "© OpenStreetMap contributors"
    }).addTo(map);

    if (!routeData || routeData.length === 0) {
        console.log("No route data");

        map.setView([homeLocation.latitude, homeLocation.longitude], 14);

        L.marker([homeLocation.latitude, homeLocation.longitude], { icon: hospitalIcon })
            .addTo(map)
            .bindPopup(`<strong>${homeLocation.name}</strong>`);

        return;
    }

    // Build full route with home at start and end
    const fullRouteData = [homeLocation, ...routeData, homeLocation];

    const waypoints = fullRouteData
        .filter(p => typeof p.latitude === "number" && typeof p.longitude === "number")
        .map(p => {
            try {
                return L.latLng(p.latitude, p.longitude);
            } catch (err) {
                console.warn("Invalid LatLng:", p, err);
                return null;
            }
        })
        .filter(wp => wp !== null);

    console.log("Waypoints:", waypoints);

    L.Routing.control({
        waypoints: waypoints,
        routeWhileDragging: false,
        show: false,
        addWaypoints: false,
        draggableWaypoints: false,
        fitSelectedRoutes: true,
        createMarker: function (i, wp) {
            const point = fullRouteData[i];
            const arrival = formatTime(point.arrivalTime);
            const departure = formatTime(point.departureTime);
            const isHome = point.name === "AHN Downtown Medical Center";

            const popupHtml = `
      <strong>${i + 1}. ${point.name}</strong><br>
      Arrive: ${arrival}<br>
      Leave: ${departure}
    `;

            if (isHome) {
                return L.marker(wp.latLng, { icon: hospitalIcon }).bindPopup(popupHtml);
            } else {
                return L.marker(wp.latLng).bindPopup(popupHtml);
            }
        }
    })
        .on('routesfound', function (e) {
            console.log("L.Symbol:", L.Symbol);
            lastRoute = e.routes[0];
            updateRouteSummary(lastRoute);
            // Extract route geometry
            const coordinates = lastRoute.coordinates.map(c => L.latLng(c.lat, c.lng));

            // Draw route line
            const routeLine = L.polyline(coordinates, { color: 'blue' }).addTo(map);

            // Add directional arrows
            if (L.Symbol && typeof L.Symbol.arrowHead === "function") {
                const decorator = L.polylineDecorator(routeLine, {
                    patterns: [
                        {
                            offset: 25,
                            repeat: 50,
                            symbol: L.Symbol.arrowHead({
                                pixelSize: 10,
                                polygon: false,
                                pathOptions: { stroke: true, color: 'blue' }
                            })
                        }
                    ]
                });
                decorator.addTo(map);
            } else {
                console.warn("PolylineDecorator plugin not available.");
            }


        })
        .addTo(map);

    document.querySelectorAll('input[name="distanceUnit"]').forEach(input => {
        input.addEventListener("change", (e) => {
            selectedUnit = e.target.value;
            if (lastRoute) updateRouteSummary(lastRoute); // refresh summary
        });
    });
});

/* EventListener to save doctor's notes */
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".save-note").forEach(button => {
        button.addEventListener("click", async () => {
            const container = button.closest("div");
            const textarea = container.querySelector(".doctor-note");
            const status = container.querySelector(".note-status");

            const patientId = textarea.dataset.patientId;
            const doctorNotes = textarea.value;

            const completeBox = container.closest("li").querySelector(".mark-complete");
            const incompleteBox = container.closest("li").querySelector(".mark-incomplete");

            const isCompleted = completeBox.checked ? true : (incompleteBox.checked ? false : null);

            const formData = new FormData();
            formData.append("patientId", patientId);
            formData.append("doctorNotes", doctorNotes);
            formData.append("completed", isCompleted);

            try {
                const response = await fetch("?handler=SaveNotes", {
                    method: "POST",
                    headers: {
                        "RequestVerificationToken": antiForgeryToken
                    },
                    body: formData
                });

                if (response.ok) {
                    status.textContent = "✔ Note saved successfully";
                    status.classList.remove("text-danger");
                    status.classList.add("text-success");
                    status.style.display = "inline";
                    setTimeout(() => status.style.display = "none", 3000);

                    // Show export button if hidden
                    const exportContainer = document.getElementById("export-container");
                    if (exportContainer && exportContainer.style.display === "none") {
                        exportContainer.style.display = "block";
                    }
                }
                else {
                    throw new Error("Save failed");
                }
            } catch {
                status.textContent = "❌ Failed to save note";
                status.classList.remove("text-success");
                status.classList.add("text-danger");
                status.style.display = "inline";
            }
        });
    });
});

/* EventListener to toggle note sections */
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".toggle-notes").forEach(button => {
        button.addEventListener("click", () => {
            const targetId = button.dataset.target;
            const allSections = document.querySelectorAll(".note-section");

            allSections.forEach(section => {
                section.style.display = section.id === targetId && section.style.display === "none" ? "block" : "none";
            });
        });
    });
});

/* EventListener to mark appointments complete/incomplete */
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll(".mark-complete").forEach(completeBox => {
        completeBox.addEventListener("change", () => {
            const patientId = completeBox.dataset.patientId;
            const item = document.getElementById(`appt-${patientId}`);
            const incompleteBox = item.querySelector(".mark-incomplete");

            if (completeBox.checked) {
                incompleteBox.checked = false;
                item.classList.add("bg-light", "border-left-success");
                item.classList.remove("border-left-danger");
            } else {
                item.classList.remove("bg-light", "border-left-success");
            }
        });
    });

    document.querySelectorAll(".mark-incomplete").forEach(incompleteBox => {
        incompleteBox.addEventListener("change", () => {
            const patientId = incompleteBox.dataset.patientId;
            const item = document.getElementById(`appt-${patientId}`);
            const completeBox = item.querySelector(".mark-complete");

            if (incompleteBox.checked) {
                completeBox.checked = false;
                item.classList.add("bg-light", "border-left-danger");
                item.classList.remove("border-left-success");
            } else {
                item.classList.remove("bg-light", "border-left-danger");
            }
        });
    });
});