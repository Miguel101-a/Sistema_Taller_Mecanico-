using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para el detalle de Cotizaciones (N:M COTIZACION ↔ PRODUCTO).
    /// Valida las líneas, exige que la cotización esté PENDIENTE y el producto ACTIVO, traduce el
    /// producto duplicado (PK compuesta) y, tras cada cambio, recalcula los totales (IVA 13%).
    /// Registra la auditoría.
    /// </summary>
    public class CN_DetalleCotizacion
    {
        private const string Modulo = "Cotizaciones";
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private static readonly string[] TiposValidos = { "SERVICIO", "REPUESTO" };

        private readonly CD_DetalleCotizacion _datos = new CD_DetalleCotizacion();
        private readonly CD_Cotizacion _cotizaciones = new CD_Cotizacion();
        private readonly CD_Producto _productos = new CD_Producto();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve las líneas de una cotización.</summary>
        public List<DetalleCotizacion> ListarPorCotizacion(int numeroCotizacion) =>
            _datos.ListarPorCotizacion(numeroCotizacion);

        /// <summary>
        /// Agrega una línea a una cotización PENDIENTE. Valida cantidad, precio y tipo, exige producto
        /// ACTIVO, traduce el producto duplicado (PK compuesta) y recalcula los totales. Audita.
        /// </summary>
        public void Agregar(DetalleCotizacion detalle, string usuarioAccion)
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

            Cotizacion cotizacion = _cotizaciones.Obtener(detalle.CotizacionNumeroCotizacion)
                ?? throw new InvalidOperationException("La cotización indicada no existe.");
            if (!CN_Cotizacion.EsEditable(cotizacion.Estado))
                throw new InvalidOperationException("La cotización ya fue aprobada o rechazada; no se pueden agregar líneas.");

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
                throw new InvalidOperationException("El producto ya está agregado a esta cotización.");
            }

            _cotizaciones.RecalcularTotales(detalle.CotizacionNumeroCotizacion);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Agregar línea {detalle.ProductoCodigo} a la cotización N° {detalle.CotizacionNumeroCotizacion}");
        }

        /// <summary>
        /// Quita una línea de una cotización PENDIENTE y recalcula los totales. Audita.
        /// </summary>
        public void Quitar(int numeroCotizacion, string productoCodigo, string usuarioAccion)
        {
            productoCodigo = (productoCodigo ?? string.Empty).Trim();

            Cotizacion cotizacion = _cotizaciones.Obtener(numeroCotizacion)
                ?? throw new InvalidOperationException("La cotización indicada no existe.");
            if (!CN_Cotizacion.EsEditable(cotizacion.Estado))
                throw new InvalidOperationException("La cotización ya fue aprobada o rechazada; no se pueden quitar líneas.");

            _datos.Eliminar(numeroCotizacion, productoCodigo);
            _cotizaciones.RecalcularTotales(numeroCotizacion);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo,
                $"Quitar línea {productoCodigo} de la cotización N° {numeroCotizacion}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta y normaliza los campos de texto de la línea.</summary>
        private static void Normalizar(DetalleCotizacion d)
        {
            d.ProductoCodigo = (d.ProductoCodigo ?? string.Empty).Trim();
            d.Descripcion = (d.Descripcion ?? string.Empty).Trim();
            d.Tipo = (d.Tipo ?? string.Empty).Trim().ToUpperInvariant();
        }

        /// <summary>Detecta violación de clave primaria/única (producto repetido en la cotización).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
