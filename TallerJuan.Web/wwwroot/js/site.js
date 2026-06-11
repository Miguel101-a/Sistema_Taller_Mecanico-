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

// ============================================================
// Modal de confirmación genérico (desactivar/eliminar).
// Un único modal en _Layout, reutilizado por todos los módulos.
// Los botones disparadores llevan:
//   class="js-confirmar"
//   data-accion="/Controlador/Eliminar"  (URL del POST)
//   data-campo="id"                       (nombre del parámetro de la PK)
//   data-valor="ABC123"                   (valor de la PK)
//   data-mensaje="¿Está seguro de desactivar al cliente Juan?"
// ============================================================
(function () {
    var modalEl = document.getElementById("sgtmjModalConfirmar");
    if (!modalEl) return;

    var form = document.getElementById("sgtmjFormConfirmar");
    var campo = document.getElementById("sgtmjCampoConfirmar");
    var mensaje = document.getElementById("sgtmjMensajeConfirmar");
    var modal = new bootstrap.Modal(modalEl);

    document.addEventListener("click", function (e) {
        var boton = e.target.closest(".js-confirmar");
        if (!boton) return;

        form.action = boton.getAttribute("data-accion") || "";
        campo.name = boton.getAttribute("data-campo") || "id";
        campo.value = boton.getAttribute("data-valor") || "";
        mensaje.textContent = boton.getAttribute("data-mensaje") || "¿Está seguro de realizar esta acción?";
        modal.show();
    });
})();
