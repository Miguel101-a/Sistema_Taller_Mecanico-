using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para el detalle de Facturas (N:M FACTURA ↔ PRODUCTO). Valida las
    /// líneas, exige que la factura esté en BORRADOR y el producto ACTIVO, traduce el producto
    /// duplicado (PK compuesta) y, tras cada cambio, recalcula los totales (IVA 13%). Registra la
    /// auditoría.
    /// </summary>
    public class CN_DetalleFactura
    {
        private const string Modulo = "Facturacion";
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private static readonly string[] TiposValidos = { "SERVICIO", "REPUESTO" };

        private readonly CD_DetalleFactura _datos = new CD_DetalleFactura();
        private readonly CD_Factura _facturas = new CD_Factura();
        private readonly CD_Producto _productos = new CD_Producto();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve las líneas de una factura.</summary>
        public List<DetalleFactura> ListarPorFactura(int numeroFactura) =>
            _datos.ListarPorFactura(numeroFactura);

        /// <summary>
        /// Agrega una línea a una factura en BORRADOR. Valida cantidad, precio y tipo, exige producto
        /// ACTIVO, traduce el producto duplicado (PK compuesta) y recalcula los totales. Audita.
        /// </summary>
        public void Agregar(DetalleFactura detalle, string usuarioAccion)
        {
            Normalizar(detalle);

            if (string.IsNullOrWhiteSpace(detalle.ProductoCodigo))
                throw new InvalidOperationException("Debe seleccionar el producto.");
            if (!TiposValidos.Contains(detalle.Tipo))
                throw new InvalidOperationException("El tipo de línea debe ser SERVICIO o REPUESTO.");
            if (detalle.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad debe ser mayor a cero.");
            if (detalle.PrecioUnitario < 0)
                throw new InvalidOperationException("El precio unitario no puede ser negativo.");

            Factura factura = _facturas.Obtener(detalle.FacturaNumeroFactura)
                ?? throw new InvalidOperationException("La factura indicada no existe.");
            if (!CN_Factura.EsEditable(factura.Estado))
                throw new InvalidOperationException("La factura ya fue emitida o anulada; no se pueden agregar líneas.");

            Producto producto = _productos.Obtener(detalle.ProductoCodigo)
                ?? throw new InvalidOperationException("El producto indicado no existe.");
            if (producto.Estado.Trim().ToUpperInvariant() != "ACTIVO")
                throw new InvalidOperationException("Solo se pueden agregar productos ACTIVOS.");

            // Si no se escribió una descripción, se usa el nombre del producto.
            if (string.IsNullOrWhiteSpace(detalle.Descripcion))
                detalle.Descripcion = producto.Nombre;

            try
            {
                _datos.Insertar(detalle);
            }
            catch (SqlException ex) when (EsDuplicado(ex))
            {
                throw new InvalidOperationException("El producto ya está agregado a esta factura.");
            }

            _facturas.RecalcularTotales(detalle.FacturaNumeroFactura);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Agregar línea {detalle.ProductoCodigo} a la factura Nº {detalle.FacturaNumeroFactura}");
        }

        /// <summary>
        /// Quita una línea de una factura en BORRADOR y recalcula los totales. Audita.
        /// </summary>
        public void Quitar(int numeroFactura, string productoCodigo, string usuarioAccion)
        {
            productoCodigo = (productoCodigo ?? string.Empty).Trim();

            Factura factura = _facturas.Obtener(numeroFactura)
                ?? throw new InvalidOperationException("La factura indicada no existe.");
            if (!CN_Factura.EsEditable(factura.Estado))
                throw new InvalidOperationException("La factura ya fue emitida o anulada; no se pueden quitar líneas.");

            _datos.Eliminar(numeroFactura, productoCodigo);
            _facturas.RecalcularTotales(numeroFactura);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Quitar línea {productoCodigo} de la factura Nº {numeroFactura}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta y normaliza los campos de texto de la línea.</summary>
        private static void Normalizar(DetalleFactura d)
        {
            d.ProductoCodigo = (d.ProductoCodigo ?? string.Empty).Trim();
            d.Descripcion = (d.Descripcion ?? string.Empty).Trim();
            d.Tipo = (d.Tipo ?? string.Empty).Trim().ToUpperInvariant();
        }

        /// <summary>Detecta violación de clave primaria/única (producto repetido en la factura).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
