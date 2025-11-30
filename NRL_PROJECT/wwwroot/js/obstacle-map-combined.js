/**
 * ObstacleMapManager
 */

// ============================================================================
// MapController
// ============================================================================
class MapController {
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

        setTimeout(() => this.map.invalidateSize(), 300);
    }

    /**
     * Add Norwegian topographic map tile layer
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
     * Add a created layer to the drawn features group
     */
    addDrawnLayer(layer) {
        this.clearDrawnFeatures();
        this.drawnFeatures.addLayer(layer);
    }

    /**
     * Restore a GeoJSON geometry to the map
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
            setTimeout(() => this.map.setZoom(19), 150);
        }
    }

    /**
     * Set view to specific coordinates
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
     */
    on(event, handler) {
        this.map.on(event, handler);
    }

    /**
     * Remove a layer from the map
     */
    removeLayer(layer) {
        if (layer) {
            this.map.removeLayer(layer);
        }
    }
}

// ============================================================================
// GeolocationHandler
// ============================================================================
class GeolocationHandler {
    constructor(mapController, options = {}) {
        this.map = mapController;
        this.options = {
            maxZoom: 19,
            timeout: 10000,
            maximumAge: 0,
            accuracyThreshold: 200,
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
     */
    locate(setView = false) {
        this.map.map.locate({
            setView: setView,
            maxZoom: this.options.maxZoom,
            enableHighAccuracy: true,
            timeout: this.options.timeout,
            maximumAge: this.options.maximumAge
        });
    }

    /**
     * Handle successful location event
     */
    handleLocationFound(event) {
        this.clearMarkers();

        if (event.accuracy > this.options.accuracyThreshold) {
            if (this.onLowAccuracy) {
                this.onLowAccuracy(event.accuracy);
            }
            this.map.setView(event.latlng, 8);
            return;
        }

        this.map.setView(event.latlng, 18);

        this.currentMarker = L.marker(event.latlng, {
            icon: this.createHelicopterIcon()
        });

        this.currentMarker.addTo(this.map.map);
        this.currentMarker.bindPopup(this.createLocationPopup(event.accuracy));
        this.currentMarker.openPopup();

        setTimeout(() => {
            if (this.currentMarker) {
                this.currentMarker.closePopup();
            }
        }, 3000);

        if (typeof event.heading === "number" && !isNaN(event.heading)) {
            this.currentMarker.setRotationAngle(event.heading);
        }

        this.accuracyCircle = L.circle(event.latlng, {
            radius: Math.min(event.accuracy / 2, 100),
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
     */
    handleLocationError(error) {
        console.warn("Geolocation error:", error.message);

        if (this.onLocationError) {
            this.onLocationError(error);
        }
    }

    /**
     * Create helicopter icon for position marker
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
     * Create popup HTML content
     */
    createLocationPopup(accuracy) {
        return `
            <div class="popup-heading">🚁 Helikopterposisjon</div>
            <div class="popup-accuracy">Nøyaktighet: ±${Math.round(accuracy)} m</div>
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
     */
    setOnLocationFound(callback) {
        this.onLocationFound = callback;
    }

    /**
     * Set callback for location errors
     */
    setOnLocationError(callback) {
        this.onLocationError = callback;
    }

    /**
     * Set callback for low accuracy warnings
     */
    setOnLowAccuracy(callback) {
        this.onLowAccuracy = callback;
    }
}

// ============================================================================
// DraftManager
// ============================================================================
class DraftManager {
    constructor(formKey) {
        this.formKey = formKey;
        this.disabled = false;
        this.onRestore = null;
    }

    /**
     * Save current form state to localStorage
     */
    save(data) {
        if (this.disabled) return;

        const { comment = "", geoJson = "" } = data;

        if (geoJson.trim().length > 0) {
            try {
                localStorage.setItem(this.formKey, JSON.stringify({
                    comment,
                    geoJson,
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
     */
    hasDraft() {
        return localStorage.getItem(this.formKey) !== null;
    }

    /**
     * Set callback for when draft is restored
     */
    setOnRestore(callback) {
        this.onRestore = callback;
    }

    /**
     * Get draft age in milliseconds
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
            accuracyToast: document.getElementById("accuracyToast"),
            accuracyToastText: document.getElementById("accuracyToastText"),
            accuracyToastDetail: document.getElementById("accuracyToastDetail")
        };
    }

    /**
     * Set up UI event listeners
     */
    setupEventListeners() {
        this.elements.btnUploadImage?.addEventListener("click", () => {
            this.elements.imageInput?.click();
        });

        this.elements.imageInput?.addEventListener("change", (e) => {
            if (e.target.files?.[0]) {
                this.showImageToast();
            }
        });

        this.elements.btnComment?.addEventListener("click", () => {
            this.openCommentSheet();
        });

        this.elements.closeComment?.addEventListener("click", () => {
            this.closeCommentSheet();
        });

        this.elements.commentSheetBackdrop?.addEventListener("click", () => {
            this.closeCommentSheet();
        });
    }

    /**
     * Set active state on a button
     */
    setActiveButton(button) {
        this.resetActiveButtons();
        button?.classList.add("ring-4", "ring-green-400", "scale-95", "bg-green-50");
        this.activeButton = button;
    }

    /**
     * Reset all button active states
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
     * Show or hide submit button based on geometry presence
     */
    updateSubmitVisibility() {
        const hasGeometry = (this.elements.geoJsonInput?.value || "").trim().length > 0;

        if (hasGeometry) {
            this.elements.submitBtn?.classList.remove("opacity-0", "pointer-events-none", "translate-y-2");
            this.elements.submitBtn?.classList.add("opacity-100", "translate-y-0");
        } else {
            this.elements.submitBtn?.classList.add("opacity-0", "pointer-events-none", "translate-y-2");
            this.elements.submitBtn?.classList.remove("opacity-100", "translate-y-0");
        }
    }

    /**
     * Open comment bottom sheet
     */
    openCommentSheet() {
        this.elements.commentSheet?.classList.remove("translate-y-full");
        this.elements.commentSheetBackdrop?.classList.remove("hidden");
    }

    /**
     * Close comment bottom sheet
     */
    closeCommentSheet() {
        this.elements.commentSheet?.classList.add("translate-y-full");
        this.elements.commentSheetBackdrop?.classList.add("hidden");
    }

    /**
     * Show toast notification
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

    showAccuracyToast(accuracyMeters) {
        if (!this.elements.accuracyToast || !this.elements.accuracyToastDetail) return;
        this.elements.accuracyToastDetail.textContent =
            `Nøyaktighet: ±${Math.round(accuracyMeters)} m`;
        this.showToast(this.elements.accuracyToast, 4000);
    }

    getComment() {
        return this.elements.commentInput?.value || "";
    }

    setComment(text) {
        if (this.elements.commentInput) {
            this.elements.commentInput.value = text;
        }
    }

    getGeoJson() {
        return this.elements.geoJsonInput?.value || "";
    }

    setGeoJson(geoJson) {
        if (this.elements.geoJsonInput) {
            this.elements.geoJsonInput.value = geoJson;
        }
    }

    resetForm() {
        this.setComment("");
        this.setGeoJson("");
        this.resetActiveButtons();
        this.updateSubmitVisibility();
    }

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
// ============================================================================
class ObstacleMapManager {
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
    }

    setupDrawingHandlers() {
        // Line limit: 2 points
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
            this.ui.updateSubmitVisibility();
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
            this.geolocation.locate(false);
        });

        this.ui.elements.btnDeleteDraft?.addEventListener('click', () => {
            this.resetReport();
        });

        this.ui.elements.submitBtn?.addEventListener('click', () => {
            this.draftManager.disable();
            this.draftManager.clear();
        });
    }

    setupFormHandlers() {
        this.ui.elements.commentInput?.addEventListener('input', () => {
            this.saveDraft();
        });

        this.ui.elements.imageInput?.addEventListener('change', () => {
            this.saveDraft();
        });
    }

    saveDraft() {
        this.draftManager.save({
            comment: this.ui.getComment(),
            geoJson: this.ui.getGeoJson()
        });
    }

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

        this.ui.updateSubmitVisibility();
    }

    resetReport() {
        this.draftManager.disable();
        this.draftManager.clear();
        this.ui.resetForm();
        this.mapController.clearDrawnFeatures();
        this.mapController.disableDrawing();
        this.geolocation.clearMarkers();
        this.ui.showCancelToast();
        this.draftManager.enable();
    }

    getMap() {
        return this.mapController.map;
    }

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
