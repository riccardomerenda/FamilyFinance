window.chartInterop = (() => {
  const charts = {};
  function render(canvasId, config) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;
    if (charts[canvasId]) { charts[canvasId].destroy(); delete charts[canvasId]; }
    charts[canvasId] = new Chart(ctx, config);
  }
  return { render };
})();

