using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para los Movimientos de Inventario (Fase 5, N:M PRODUCTO ↔ ORDEN_TRABAJO).
    /// Valida tipo/cantidad, exige producto ACTIVO y orden no ENTREGADA, deja que el SP mueva el
    /// stock en su transacción (traduce su SqlException al usuario) y registra la auditoría.
    /// </summary>
    public class CN_MovimientoInventario
    {
        private const string Modulo = "Inventario";

        private static readonly string[] TiposValidos = { "INGRESO", "SALIDA" };

        private readonly CD_MovimientoInventario _datos = new CD_MovimientoInventario();
        private readonly CD_Producto _productos = new CD_Producto();
        private readonly CD_OrdenTrabajo _ordenes = new CD_OrdenTrabajo();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todos los movimientos de inventario (más recientes primero).</summary>
        public List<MovimientoInventario> Listar() => _datos.Listar();

        /// <summary>Devuelve los movimientos de un producto.</summary>
        public List<MovimientoInventario> ListarPorProducto(string productoCodigo) =>
            _datos.ListarPorProducto(productoCodigo);

        /// <summary>
        /// Registra un movimiento de inventario. Valida tipo y cantidad, exige que el producto esté
        /// ACTIVO y que la orden no esté ENTREGADA. El SP descuenta/aumenta el stock en su transacción;
        /// si lanza un error (stock insuficiente, cantidad inválida) se traslada su mensaje al usuario.
        /// Registra la auditoría.
        /// </summary>
        public void Registrar(MovimientoInventario movimiento, string usuarioAccion)
        {
            Normalizar(movimiento);

            if (string.IsNullOrWhiteSpace(movimiento.ProductoCodigo))
                throw new InvalidOperationException("Debe seleccionar el producto.");
            if (movimiento.OrdenTrabajoNumeroOrden <= 0)
                throw new InvalidOperationException("Debe seleccionar la orden de trabajo asociada.");
            if (!TiposValidos.Contains(movimiento.Tipo))
                throw new InvalidOperationException("El tipo de movimiento debe ser INGRESO o SALIDA.");
            if (movimiento.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");

            Producto producto = _productos.Obtener(movimiento.ProductoCodigo)
                ?? throw new InvalidOperationException("El producto indicado no existe.");
            if (producto.Estado.Trim().ToUpperInvariant() != "ACTIVO")
                throw new InvalidOperationException("Solo se pueden mover productos ACTIVOS.");

            OrdenTrabajo orden = _ordenes.Obtener(movimiento.OrdenTrabajoNumeroOrden)
                ?? throw new InvalidOperationException("La orden de trabajo indicada no existe.");
            if (CN_OrdenTrabajo.EsEntregada(orden.Estado))
                throw new InvalidOperationException("La orden ya fue entregada; no admite movimientos de inventario.");

            try
            {
                _datos.Insertar(movimiento);
            }
            catch (SqlException ex)
            {
                // El SP lanza mensajes en español (stock insuficiente, etc.); se muestran tal cual.
                throw new InvalidOperationException(ex.Message);
            }

            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"{movimiento.Tipo} de {movimiento.Cantidad} x {movimiento.ProductoCodigo} (orden N° {movimiento.OrdenTrabajoNumeroOrden})");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta y normaliza los campos de texto del movimiento.</summary>
        private static void Normalizar(MovimientoInventario m)
        {
            m.ProductoCodigo = (m.ProductoCodigo ?? string.Empty).Trim();
            m.Tipo = (m.Tipo ?? string.Empty).Trim().ToUpperInvariant();
            m.Motivo = (m.Motivo ?? string.Empty).Trim();
        }
    }
}
