

// ============================================================================
// MapController
// Manages the Leaflet map instance and basic drawing operations.
// ============================================================================
class MapController {
    /**
     * @param {string} elementId - The ID of the map container element.
     * @param {object} [options] - Configuration options.
     */
    constructor(elementId, options = {}) {
        this.elementId = elementId;
        this.options = {
            defaultCenter: [58.333, 8.233],
            defaultZoom: 5,
            maxZoom: 19,
            ...options
        };

        this.map = null;
        this.drawnFeatures = null;
        this.activeButton = null;

        this.init();
    }

    /**
     * Initialize the map and set up layers
     */
    init() {
        this.map = L.map(this.elementId, {
            zoomControl: false,
            touchZoom: true,
            scrollWheelZoom: true,
            doubleClickZoom: true,
            boxZoom: false
        }).setView(this.options.defaultCenter, this.options.defaultZoom);

        this.addTileLayer();
        this.setupDrawingControls();
        this.drawnFeatures = L.featureGroup().addTo(this.map);

        // Invalidate size ensures map displays correctly after layout changes
        setTimeout(() => this.map.invalidateSize(), 300);
    }

    /**
     * Add Norwegian topographic map tile layer (Kartverket)
     */
    addTileLayer() {
        L.tileLayer.wms('https://wms.geonorge.no/skwms1/wms.topo', {
            layers: 'topo',
            format: 'image/png',
            transparent: false,
            attribution: '© Kartverket'
        }).addTo(this.map);
    }

    /**
     * Configure Geoman drawing controls (hide default UI)
     */
    setupDrawingControls() {
        this.map.pm.addControls({
            drawMarker: false,
            drawPolyline: false,
            editMode: false,
            removalMode: false
        });

        // Ensure default toolbar is completely hidden
        if (this.map.pm?.Toolbar) {
            this.map.pm.Toolbar.removeControls();
        }
    }

    /**
     * Enable marker drawing mode
     */
    enableMarkerDrawing() {
        this.map.pm.disableDraw();
        this.map.pm.enableDraw('Marker', { snappable: false });
    }

    /**
     * Enable line drawing mode with 2-point limit
     */
    enableLineDrawing() {
        this.map.pm.disableDraw();
        this.map.pm.enableDraw('Line', {
            snappable: false,
            pathOptions: {
                color: 'orange',
                weight: 4
            }
        });
    }

    /**
     * Disable all drawing modes
     */
    disableDrawing() {
        this.map.pm.disableDraw();
    }

    /**
     * Clear all drawn features from the map
     */
    clearDrawnFeatures() {
        this.drawnFeatures.clearLayers();
    }

    /**
     * Add a created layer to the drawn features group, clearing previous ones.
     * @param {L.Layer} layer - The layer to add.
     */
    addDrawnLayer(layer) {
        this.clearDrawnFeatures();
        this.drawnFeatures.addLayer(layer);
    }

    /**
     * Restore a GeoJSON geometry to the map and center the view.
     * @param {GeoJSON.Geometry} geometry - The geometry object to restore.
     */
    restoreGeometry(geometry) {
        this.clearDrawnFeatures();

        if (geometry.type === "Point") {
            const [lng, lat] = geometry.coordinates;
            const marker = L.marker([lat, lng]);
            this.drawnFeatures.addLayer(marker);
            marker.addTo(this.map);
            this.map.setView([lat, lng], 19);
        } else if (geometry.type === "LineString") {
            const latlngs = geometry.coordinates.map(([lng, lat]) => [lat, lng]);
            const polyline = L.polyline(latlngs, {
                color: "orange",
                weight: 4
            });
            this.drawnFeatures.addLayer(polyline);
            polyline.addTo(this.map);
            this.map.fitBounds(polyline.getBounds());
            setTimeout(() => this.map.setZoom(17), 150);
        }
    }

    /**
     * Set view to specific coordinates
     * @param {L.LatLng} latlng - The latitude and longitude to center on.
     * @param {number} zoom - The target zoom level.
     */
    setView(latlng, zoom) {
        this.map.setView(latlng, zoom);
    }

    /**
     * Invalidate map size (useful after DOM changes)
     */
    invalidateSize() {
        this.map.invalidateSize();
    }

    /**
     * Register event handler for map events
     * @param {string} event - The event name.
     * @param {function} handler - The event handler function.
     */
    on(event, handler) {
        this.map.on(event, handler);
    }

    /**
     * Remove a layer from the map
     * @param {L.Layer} layer - The layer to remove.
     */
    removeLayer(layer) {
        if (layer) {
            this.map.removeLayer(layer);
        }
    }
}

// ============================================================================
// GeolocationHandler
// Handles finding the user's current location and displaying it on the map.
// ============================================================================
class GeolocationHandler {
    /**
     * @param {MapController} mapController - The map controller instance.
     * @param {object} [options] - Configuration options.
     */
    constructor(mapController, options = {}) {
        this.map = mapController;
        this.options = {
            maxZoom: 19,
            timeout: 10000,
            maximumAge: 0,
            accuracyThreshold: 200, // Meters
            ...options
        };

        this.currentMarker = null;
        this.accuracyCircle = null;
        this.onLocationFound = null;
        this.onLocationError = null;
        this.onLowAccuracy = null;

        this.setupEventHandlers();
    }

    /**
     * Set up map location event handlers
     */
    setupEventHandlers() {
        this.map.on("locationfound", (e) => this.handleLocationFound(e));
        this.map.on("locationerror", (e) => this.handleLocationError(e));
    }

    /**
     * Request user's current location
     * @param {boolean} [setView=false] - If true, the map view is adjusted to the location.
     * @param {number} [zoom=16] - Zoom level to use if setView is true.
     */
    locate(setView = false, zoom = 16) {
        // Clear any previous location markers before starting a new search
        this.clearMarkers();

        this.map.map.locate({
            setView: setView,
            maxZoom: this.options.maxZoom,
            enableHighAccuracy: true,
            timeout: this.options.timeout,
            maximumAge: this.options.maximumAge
        });
    }

    /**
     * Handle successful location event, showing marker and accuracy circle.
     * @param {L.LocationEvent} event - The Leaflet location event.
     */
    handleLocationFound(event) {
        this.clearMarkers();

        // Check for low accuracy warning
        if (event.accuracy > this.options.accuracyThreshold) {
            if (this.onLowAccuracy) {
                this.onLowAccuracy(event.accuracy);
            }
            this.map.setView(event.latlng, 8); // Zoom out if accuracy is poor
            return;
        }

        this.map.setView(event.latlng, 18);

        this.currentMarker = L.marker(event.latlng, {
            icon: this.createHelicopterIcon()
        });

        this.currentMarker.addTo(this.map.map);
        this.currentMarker.bindPopup(this.createLocationPopup(event.accuracy));
        this.currentMarker.openPopup();

        // Close popup after a short delay
        setTimeout(() => {
            if (this.currentMarker) {
                this.currentMarker.closePopup();
            }
        }, 3000);

        // Set rotation for the icon if heading data is available
        if (typeof event.heading === "number" && !isNaN(event.heading)) {
            // Note: Marker rotation requires a Leaflet extension (leaflet-rotatedmarker)
            if (this.currentMarker.setRotationAngle) {
                this.currentMarker.setRotationAngle(event.heading);
            }
        }

        // Add accuracy circle
        this.accuracyCircle = L.circle(event.latlng, {
            radius: Math.min(event.accuracy / 2, 100), // Max radius visual limit
            color: "blue",
            weight: 2,
            opacity: 0.6,
            fillOpacity: 0.15
        });

        this.accuracyCircle.addTo(this.map.map);

        if (this.onLocationFound) {
            this.onLocationFound(event);
        }
    }

    /**
     * Handle location error event
     * @param {L.ErrorEvent} error - The Leaflet error event.
     */
    handleLocationError(error) {
        console.warn("Geolocation error:", error.message);

        if (this.onLocationError) {
            this.onLocationError(error);
        }
    }

    /**
     * Create custom icon for position marker
     */
    createHelicopterIcon() {
        return L.icon({
            iconUrl: '/icons/helicopter.svg',
            iconSize: [45, 45],
            iconAnchor: [19, 19],
            popupAnchor: [0, -12]
        });
    }

    /**
     * Create popup HTML content for location marker
     * @param {number} accuracy - The accuracy in meters.
     * @returns {string} HTML content.
     */
    createLocationPopup(accuracy) {
        return `
            <div class="popup-heading">🚁 Helicopter Position</div>
            <div class="popup-accuracy">Accuracy: ±${Math.round(accuracy)} m</div>
        `;
    }

    /**
     * Remove current position markers from map
     */
    clearMarkers() {
        if (this.currentMarker) {
            this.map.removeLayer(this.currentMarker);
            this.currentMarker = null;
        }

        if (this.accuracyCircle) {
            this.map.removeLayer(this.accuracyCircle);
            this.accuracyCircle = null;
        }
    }

    /**
     * Set callback for successful location
     * @param {function} callback - The callback function.
     */
    setOnLocationFound(callback) {
        this.onLocationFound = callback;
    }

    /**
     * Set callback for location errors
     * @param {function} callback - The callback function.
     */
    setOnLocationError(callback) {
        this.onLocationError = callback;
    }

    /**
     * Set callback for low accuracy warnings
     * @param {function} callback - The callback function.
     */
    setOnLowAccuracy(callback) {
        this.onLowAccuracy = callback;
    }
}

// ============================================================================
// DraftManager
// Handles saving, restoring, and clearing form data (comment and GeoJSON) 
// using localStorage.
// ============================================================================
class DraftManager {
    /**
     * @param {string} formKey - The unique key for localStorage.
     */
    constructor(formKey) {
        this.formKey = formKey;
        this.disabled = false;
        this.onRestore = null;
    }

    /**
     * Save current form state to localStorage
     * @param {object} data - Object containing comment, geoJson, and hasImage boolean.
     */
    save(data) {
        if (this.disabled) return;

        const { comment = "", geoJson = "", hasImage = false } = data;

        // Only save a draft if a geometric feature (GeoJSON) exists
        if (geoJson.trim().length > 0) {
            try {
                localStorage.setItem(this.formKey, JSON.stringify({
                    comment,
                    geoJson,
                    hasImage, // Store only the presence of the image (since we can't store the file itself)
                    timestamp: Date.now()
                }));
            } catch (error) {
                console.error("Failed to save draft:", error);
            }
        } else {
            this.clear();
        }
    }

    /**
     * Restore saved draft from localStorage
     * @returns {object|null} The restored draft data or null.
     */
    restore() {
        const saved = localStorage.getItem(this.formKey);
        if (!saved) return null;

        try {
            const data = JSON.parse(saved);

            if (!data.geoJson) {
                this.clear();
                return null;
            }

            // Ensure the restored object has the 'hasImage' property (default to false if old draft)
            data.hasImage = data.hasImage === true;

            if (this.onRestore) {
                this.onRestore(data);
            }

            return data;
        } catch (error) {
            console.error("Failed to restore draft:", error);
            this.clear();
            return null;
        }
    }

    /**
     * Clear saved draft from localStorage
     */
    clear() {
        localStorage.removeItem(this.formKey);
    }

    /**
     * Disable draft autosaving
     */
    disable() {
        this.disabled = true;
    }

    /**
     * Enable draft autosaving
     */
    enable() {
        this.disabled = false;
    }

    /**
     * Check if a draft exists
     * @returns {boolean} True if a draft exists.
     */
    hasDraft() {
        return localStorage.getItem(this.formKey) !== null;
    }

    /**
     * Set callback for when draft is restored
     * @param {function} callback - The callback function.
     */
    setOnRestore(callback) {
        this.onRestore = callback;
    }

    /**
     * Get draft age in milliseconds
     * @returns {number|null} The age in milliseconds or null if no draft exists.
     */
    getDraftAge() {
        const saved = localStorage.getItem(this.formKey);
        if (!saved) return null;

        try {
            const data = JSON.parse(saved);
            if (data.timestamp) {
                return Date.now() - data.timestamp;
            }
        } catch (error) {
            console.error("Failed to get draft age:", error);
        }

        return null;
    }
}

// ============================================================================
// UIController
// Manages DOM interactions, button states, toasts, and the comment sheet.
// ============================================================================
class UIController {
    constructor() {
        this.activeButton = null;
        this.elements = this.cacheElements();
        this.setupEventListeners();
    }

    /**
     * Cache DOM element references
     */
    cacheElements() {
        return {
            btnDrawPoint: document.getElementById("btnDrawPoint"),
            btnDrawLine: document.getElementById("btnDrawLine"),
            btnLocate: document.getElementById("btnLocate"),
            btnUploadImage: document.getElementById("btnUploadImage"),
            btnComment: document.getElementById("btnComment"),
            btnDeleteDraft: document.getElementById("btnDeleteDraft"),
            submitBtn: document.getElementById("submitBtn"),
            geoJsonInput: document.getElementById("GeoJsonCoordinates"),
            commentInput: document.querySelector('[name="ObstacleComment"]'),
            imageInput: document.getElementById("imageInput"),
            commentSheet: document.getElementById("commentSheet"),
            commentSheetBackdrop: document.getElementById("commentSheetBackdrop"),
            closeComment: document.getElementById("closeComment"),
            restoreToast: document.getElementById("restoreToast"),
            cancelToast: document.getElementById("cancelToast"),
            imageToast: document.getElementById("imageToast"),
            commentToast: document.getElementById("commentToast"),
            errorToast: document.getElementById("errorToast"),
            errorToastText: document.getElementById("errorToastText"),
            accuracyToast: document.getElementById("accuracyToast"),
            accuracyToastText: document.getElementById("accuracyToastText"),
            accuracyToastDetail: document.getElementById("accuracyToastDetail")
        };
    }

    /**
     * Set up UI event listeners
     */
    setupEventListeners() {
        // Image upload with validation
        this.elements.btnUploadImage?.addEventListener("click", () => {
            this.elements.imageInput?.click();
        });

        this.elements.imageInput?.addEventListener("change", (e) => {
            const file = e.target.files?.[0];
            if (!file) {
                // Image cleared or cancelled
                return;
            }

            // Client-side validation
            const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/heic', 'image/webp'];
            const allowedExtensions = ['.jpg', '.jpeg', '.png', '.heic', '.webp'];
            const maxSize = 5 * 1024 * 1024; // 5MB

            const fileName = file.name.toLowerCase();
            const hasValidExtension = allowedExtensions.some(ext => fileName.endsWith(ext));

            // Check MIME type AND extension
            if (!allowedTypes.includes(file.type) && !hasValidExtension) {
                this.showErrorToast('Only .jpg, .jpeg, .png, .heic, .webp are allowed');
                e.target.value = ''; // Clear invalid file
                return;
            }

            if (file.size > maxSize) {
                this.showErrorToast('The image must be less than 5MB');
                e.target.value = ''; // Clear invalid file
                return;
            }

            // Success
            this.showImageToast();
        });

        // Comment sheet controls
        this.elements.btnComment?.addEventListener("click", () => {
            this.openCommentSheet();
        });

        this.elements.closeComment?.addEventListener("click", () => {
            this.closeCommentSheet();
        });

        this.elements.commentSheetBackdrop?.addEventListener("click", () => {
            this.closeCommentSheet();
        });

        // Fjernet den problematiske placeholder-logikken for input her.
        // Input events skal kun brukes til å fange endringer for autosave.
        this.elements.commentInput?.addEventListener("input", (e) => {
            // Handled in ObstacleMapManager.setupFormHandlers for draft saving
        });
    }

    /**
     * Set active state on a drawing/action button
     * @param {HTMLElement} button - The button element to activate.
     */
    setActiveButton(button) {
        this.resetActiveButtons();
        button?.classList.add("ring-4", "ring-green-400", "scale-95", "bg-green-50");
        this.activeButton = button;
    }

    /**
     * Reset all drawing/action button active states
     */
    resetActiveButtons() {
        [
            this.elements.btnDrawPoint,
            this.elements.btnDrawLine,
            this.elements.btnLocate
        ].forEach(btn => {
            btn?.classList.remove("ring-4", "ring-green-400", "scale-95", "bg-green-50");
        });
        this.activeButton = null;
    }

    /**
     * Show or hide the submit button based on geometry presence.
     * The button remains permanently visible when GeoJSON is present.
     */
    updateSubmitVisibility() {
        const geoJsonExists = this.elements.geoJsonInput?.value.trim().length > 0;
        const submitBtn = this.elements.submitBtn;

        if (!submitBtn) return;

        // Remove disabling classes if GeoJSON exists (drawn geometry)
        if (geoJsonExists) {
            submitBtn.classList.remove("opacity-0", "translate-y-2", "pointer-events-none");
        } else {
            // Add disabling classes if GeoJSON is missing
            submitBtn.classList.add("opacity-0", "translate-y-2", "pointer-events-none");
        }
    }

    /**
     * Open comment bottom sheet
     */
    openCommentSheet() {
        this.elements.commentSheet?.classList.remove("translate-y-full");
        this.elements.commentSheetBackdrop?.classList.remove("hidden");

        // Auto-focus textarea after animation
        setTimeout(() => {
            this.elements.commentInput?.focus();
        }, 200);
    }

    /**
     * Close comment bottom sheet
     */
    closeCommentSheet() {
        this.elements.commentSheet?.classList.add("translate-y-full");
        this.elements.commentSheetBackdrop?.classList.add("hidden");

        // Show confirmation if there's actual user text (not placeholder)
        const text = this.elements.commentInput?.value.trim() || "";
        const placeholder = "(Valgfritt) Beskriv hinderet her.";

        if (text.length > 0 && text !== placeholder) {
            this.showCommentToast();
        }
    }

    /**
     * Trigger comment sheet to open (for external calls, e.g., after drawing)
     */
    triggerCommentSheet() {
        this.openCommentSheet();
    }

    /**
     * Show toast notification
     * @param {HTMLElement} toast - The toast element.
     * @param {number} [duration=5000] - Duration in milliseconds.
     */
    showToast(toast, duration = 5000) {
        if (!toast) return;
        toast.style.opacity = "1";
        setTimeout(() => { toast.style.opacity = "0"; }, duration);
    }

    showRestoreToast() {
        this.showToast(this.elements.restoreToast);
    }

    showCancelToast() {
        this.showToast(this.elements.cancelToast);
    }

    showImageToast() {
        this.showToast(this.elements.imageToast, 3500);
    }

    showCommentToast() {
        this.showToast(this.elements.commentToast, 3500);
    }

    /**
     * Show a custom error message toast
     * @param {string} message - The error message.
     */
    showErrorToast(message) {
        if (!this.elements.errorToast || !this.elements.errorToastText) return;
        this.elements.errorToastText.textContent = message;
        this.showToast(this.elements.errorToast, 5000);
    }

    /**
     * Show a low-accuracy warning toast
     * @param {number} accuracyMeters - The measured accuracy.
     */
    showAccuracyToast(accuracyMeters) {
        if (!this.elements.accuracyToast || !this.elements.accuracyToastDetail) return;
        this.elements.accuracyToastDetail.textContent =
            `Accuracy: ±${Math.round(accuracyMeters)} m`;
        this.showToast(this.elements.accuracyToast, 4000);
    }

    /**
     * Get the current obstacle comment.
     * @returns {string} The comment text.
     */
    getComment() {
        return this.elements.commentInput?.value || "";
    }

    /**
     * Set the obstacle comment field value.
     * @param {string} text - The text to set.
     */
    setComment(text) {
        if (this.elements.commentInput) {
            this.elements.commentInput.value = text;
        }
    }

    /**
     * Check if an image file has been selected.
     * @returns {boolean} True if a file exists in the input field.
     */
    hasImageFile() {
        return (this.elements.imageInput?.files?.length || 0) > 0;
    }

    /**
     * Clear the selected image file from the input field.
     * This is necessary when resetting the report, as the file reference
     * must be removed for the form to truly be empty.
     */
    clearImageFile() {
        if (this.elements.imageInput) {
            this.elements.imageInput.value = ''; // Clears the file reference
        }
    }

    /**
     * Get the GeoJSON string from the hidden input.
     * @returns {string} The GeoJSON string.
     */
    getGeoJson() {
        return this.elements.geoJsonInput?.value || "";
    }

    /**
     * Set the GeoJSON string in the hidden input.
     * @param {string} geoJson - The GeoJSON string.
     */
    setGeoJson(geoJson) {
        if (this.elements.geoJsonInput) {
            this.elements.geoJsonInput.value = geoJson;
        }
    }

    /**
     * Reset all form inputs and UI states.
     */
    resetForm() {
        this.setComment("");
        this.setGeoJson("");
        this.clearImageFile(); // VIKTIG: Nullstill filen
        this.resetActiveButtons();
        this.updateSubmitVisibility(); // Ensure button is hidden after reset
    }

    /**
     * Get a specific button element by name.
     * @param {string} name - 'drawPoint', 'drawLine', or 'locate'.
     * @returns {HTMLElement|null} The button element.
     */
    getButton(name) {
        const buttonMap = {
            drawPoint: this.elements.btnDrawPoint,
            drawLine: this.elements.btnDrawLine,
            locate: this.elements.btnLocate
        };
        return buttonMap[name] || null;
    }
}

// ============================================================================
// ObstacleMapManager - Main Orchestrator
// Coordinates all components and handles application workflow.
// ============================================================================
class ObstacleMapManager {
    /**
     * @param {string} mapElementId - The ID of the map container.
     * @param {object} [options] - Configuration options.
     */
    constructor(mapElementId, options = {}) {
        this.options = {
            formKey: "ObstacleReportForm",
            autoLocateOnStart: true,
            ...options
        };

        this.restoredFromDraft = false;

        this.mapController = new MapController(mapElementId);
        this.geolocation = new GeolocationHandler(this.mapController);
        this.draftManager = new DraftManager(this.options.formKey);
        this.ui = new UIController();

        this.init();
    }

    /**
     * Initialize the manager and set up all event handlers
     */
    init() {
        this.setupGeolocationCallbacks();
        this.setupDrawingHandlers();
        this.setupButtonHandlers();
        this.setupFormHandlers();
        this.restoreDraft();

        if (!this.restoredFromDraft && this.options.autoLocateOnStart) {
            this.geolocation.locate(true);
        }
    }

    setupGeolocationCallbacks() {
        this.geolocation.setOnLowAccuracy((accuracy) => {
            this.ui.showAccuracyToast(accuracy);
        });

        this.geolocation.setOnLocationError((error) => {
            console.warn("Location unavailable - using fallback");
        });

        // Callback for locating the map to user position after resetting report
        this.geolocation.setOnLocationFound((e) => {
            console.log("Location found:", e.latlng);
        });
    }

    setupDrawingHandlers() {
        // Line limit: 2 points, drawing is over after 2 points
        this.mapController.on('pm:drawstart', ({ shape, workingLayer }) => {
            if (shape === 'Line') {
                workingLayer.on('pm:vertexadded', () => {
                    const pts = workingLayer.getLatLngs();
                    if (pts.length >= 2) {
                        if (this.mapController.map.pm?.Draw?.Line?._finishShape)
                            this.mapController.map.pm.Draw.Line._finishShape();
                        else
                            this.mapController.map.pm.disableDraw();
                    }
                });
            }
        });

        // Handle created geometry
        this.mapController.on('pm:create', (e) => {
            this.mapController.addDrawnLayer(e.layer);
            this.mapController.disableDrawing();

            const geoJson = JSON.stringify(e.layer.toGeoJSON().geometry);
            this.ui.setGeoJson(geoJson);

            this.ui.resetActiveButtons();
            this.saveDraft();

            // Show the submit button immediately after drawing
            this.ui.updateSubmitVisibility();

            // Auto-open comment sheet after drawing
            this.ui.triggerCommentSheet();
        });
    }

    setupButtonHandlers() {
        this.ui.getButton('drawPoint')?.addEventListener('click', () => {
            this.ui.setActiveButton(this.ui.getButton('drawPoint'));
            this.mapController.enableMarkerDrawing();
        });

        this.ui.getButton('drawLine')?.addEventListener('click', () => {
            this.ui.setActiveButton(this.ui.getButton('drawLine'));
            this.mapController.enableLineDrawing();
        });

        this.ui.getButton('locate')?.addEventListener('click', () => {
            this.ui.resetActiveButtons();
            this.geolocation.locate(true); // Center view when locate button is pressed
        });

        this.ui.elements.btnDeleteDraft?.addEventListener('click', () => {
            this.resetReport();
        });

        // Ensure draft is cleared before form submission
        this.ui.elements.submitBtn?.addEventListener('click', () => {
            this.draftManager.disable();
            this.draftManager.clear();
        });
    }

    setupFormHandlers() {
        // Save draft on comment or image change
        this.ui.elements.commentInput?.addEventListener('input', () => {
            this.saveDraft();
        });

        this.ui.elements.imageInput?.addEventListener('change', () => {
            // This is primarily to trigger draft saving of the 'hasImage' status
            this.saveDraft();
        });
    }

    /**
     * Save current form data to local storage.
     */
    saveDraft() {
        this.draftManager.save({
            comment: this.ui.getComment(),
            geoJson: this.ui.getGeoJson(),
            hasImage: this.ui.hasImageFile() // Store ONLY the presence status
        });
    }

    /**
     * Restore a draft from local storage, if one exists.
     */
    restoreDraft() {
        const draft = this.draftManager.restore();
        if (!draft) return;

        this.ui.setGeoJson(draft.geoJson);
        if (draft.comment) {
            this.ui.setComment(draft.comment);
        }


        try {
            const geometry = JSON.parse(draft.geoJson);
            this.mapController.restoreGeometry(geometry);
            this.restoredFromDraft = true;
            this.ui.showRestoreToast();
        } catch (error) {
            console.error("Failed to restore geometry:", error);
            this.draftManager.clear();
        }

        // Ensure the submit button is visible after draft restore
        this.ui.updateSubmitVisibility();
    }

    /**
     * Reset the entire obstacle report state and center the map to current location.
     */
    resetReport() {
        this.draftManager.disable();
        this.draftManager.clear();
        this.ui.resetForm();
        this.mapController.clearDrawnFeatures();
        this.mapController.disableDrawing();
        this.geolocation.clearMarkers();
        this.ui.showCancelToast();
        this.geolocation.locate(true, 16); // locate(setView=true, zoom=16)

        this.draftManager.enable();
    }

    /**
     * @returns {L.Map} The Leaflet map instance.
     */
    getMap() {
        return this.mapController.map;
    }

    /**
     * @returns {object} The current form data (comment and GeoJSON).
     */
    getFormData() {
        return {
            comment: this.ui.getComment(),
            geoJson: this.ui.getGeoJson()
        };
    }
}

// ============================================================================
// Initialize on window load
// ============================================================================
window.addEventListener('load', () => {
    window.obstacleMap = new ObstacleMapManager('mapDraw');
});
