using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para los Diagnósticos de una orden de trabajo (Fase 4).
    /// Valida la descripción y los valores opcionales, y registra la auditoría.
    /// </summary>
    public class CN_Diagnostico
    {
        private const string Modulo = "Ordenes";

        private readonly CD_Diagnostico _datos = new CD_Diagnostico();
        private readonly CD_OrdenTrabajo _ordenes = new CD_OrdenTrabajo();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve los diagnósticos de una orden (más recientes primero).</summary>
        public List<Diagnostico> ListarPorOrden(int numeroOrden) => _datos.ListarPorOrden(numeroOrden);

        /// <summary>
        /// Registra un diagnóstico sobre una orden. La descripción es obligatoria; tiempo y costo
        /// son opcionales pero, si vienen, deben ser ≥ 0. No se permite sobre una orden ENTREGADA.
        /// Registra la auditoría y devuelve el ID generado.
        /// </summary>
        public int Agregar(Diagnostico diagnostico, string usuarioAccion)
        {
            diagnostico.Descripcion = (diagnostico.Descripcion ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(diagnostico.Descripcion))
                throw new InvalidOperationException("La descripción del diagnóstico es obligatoria.");
            if (diagnostico.TiempoEstimado.HasValue && diagnostico.TiempoEstimado < 0)
                throw new InvalidOperationException("El tiempo estimado no puede ser negativo.");
            if (diagnostico.CostoEstimado.HasValue && diagnostico.CostoEstimado < 0)
                throw new InvalidOperationException("El costo estimado no puede ser negativo.");

            OrdenTrabajo orden = _ordenes.Obtener(diagnostico.OrdenTrabajoNumeroOrden)
                ?? throw new InvalidOperationException("La orden de trabajo indicada no existe.");

            if (CN_OrdenTrabajo.EsEntregada(orden.Estado))
                throw new InvalidOperationException("La orden ya fue entregada; no se pueden agregar diagnósticos.");

            int id = _datos.Insertar(diagnostico);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Agregar diagnóstico (Id={id}) a la orden N° {diagnostico.OrdenTrabajoNumeroOrden}");
            return id;
        }
    }
}
