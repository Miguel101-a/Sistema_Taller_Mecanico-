// ============================================================
// SGTMJ - Buscador en vivo de tablas (reutilizable, vanilla JS)
// ------------------------------------------------------------
// Filtra las filas de una tabla en el cliente, sin recargar la página,
// y actualiza un contador de registros visibles.
//
// Uso en la vista:
//   <input class="sgtmj-buscador" data-tabla="idTabla" data-contador="idContador" />
//   <table id="idTabla"> ... <tbody> filas </tbody> </table>
//   <span id="idContador"></span>
// ============================================================
(function () {
    // Aplica el filtro a una tabla según el texto buscado.
    function filtrar(input) {
        var idTabla = input.getAttribute("data-tabla");
        var tabla = document.getElementById(idTabla);
        if (!tabla) return;

        var texto = (input.value || "").toLowerCase().trim();
        var filas = tabla.tBodies.length ? tabla.tBodies[0].rows : [];
        var visibles = 0;

        for (var i = 0; i < filas.length; i++) {
            var fila = filas[i];
            // Las filas marcadas como "sin datos" no se filtran.
            if (fila.hasAttribute("data-sin-filtro")) {
                continue;
            }
            var contenido = fila.textContent.toLowerCase();
            var coincide = texto === "" || contenido.indexOf(texto) !== -1;
            fila.classList.toggle("sgtmj-oculto", !coincide);
            if (coincide) visibles++;
        }

        var idContador = input.getAttribute("data-contador");
        if (idContador) {
            var contador = document.getElementById(idContador);
            if (contador) {
                contador.textContent = visibles + (visibles === 1 ? " registro" : " registros");
            }
        }
    }

    // Engancha todos los buscadores de la página al cargar.
    document.addEventListener("DOMContentLoaded", function () {
        var inputs = document.querySelectorAll(".sgtmj-buscador[data-tabla]");
        inputs.forEach(function (input) {
            filtrar(input); // inicializa el contador
            input.addEventListener("input", function () { filtrar(input); });
        });
    });
})();
