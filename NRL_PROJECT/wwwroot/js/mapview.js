const map = L.map('mapDraw').setView([58.333, 8.233], 13);
L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(map);

map.pm.addControls({
    position: 'topleft',
    drawMarker: true,
    drawPolyline: true,
    drawPolygon: false,
    drawCircle: false,
    drawRectangle: false,
    editMode: true,
    removalMode: true
});

map.on('pm:create', e => {
    const geojson = e.layer.toGeoJSON();
    document.getElementById('geoInput').value = JSON.stringify(geojson.geometry.coordinates);
});

map.on('pm:remove', () => {
    document.getElementById('geoInput').value = '';
});
