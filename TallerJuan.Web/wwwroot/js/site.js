// ============================================================
// SGTMJ - JavaScript del sitio
// ============================================================

// Colapsar/expandir el menú lateral (sidebar).
// En escritorio alterna el modo "solo iconos"; en móvil abre/cierra el panel deslizante.
(function () {
    var app = document.getElementById("sgtmjApp");
    var boton = document.getElementById("sgtmjToggle");
    if (!app || !boton) return;

    boton.addEventListener("click", function () {
        // En pantallas chicas se desliza el sidebar; en grandes se colapsa a solo iconos.
        if (window.matchMedia("(max-width: 768px)").matches) {
            app.classList.toggle("sgtmj-movil-abierto");
        } else {
            app.classList.toggle("sgtmj-colapsado");
        }
    });
})();
