using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para Clientes. Envuelve a CD_Cliente, aplica validaciones,
    /// traduce errores de BD a mensajes en español y registra la auditoría reutilizando
    /// CD_Acceso.RegistrarAuditoria. La capa Web SIEMPRE pasa por aquí.
    /// </summary>
    public class CN_Cliente
    {
        private const string Modulo = "Clientes";
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private readonly CD_Cliente _datos = new CD_Cliente();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todos los clientes.</summary>
        public List<Cliente> Listar() => _datos.Listar();

        /// <summary>Devuelve un cliente por su cédula, o null si no existe.</summary>
        public Cliente? Obtener(string cedula) => _datos.Obtener(cedula);

        /// <summary>
        /// Crea un cliente nuevo. Valida campos obligatorios, traduce la cédula duplicada
        /// y registra la auditoría.
        /// </summary>
        public void Crear(Cliente cliente, string usuarioAccion)
        {
            Normalizar(cliente);
            Validar(cliente);

            try
            {
                _datos.Insertar(cliente);
                _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Crear cliente {cliente.Cedula}");
            }
            catch (SqlException ex) when (EsDuplicado(ex))
            {
                throw new InvalidOperationException("Ya existe un cliente con esa cédula.");
            }
        }

        /// <summary>Actualiza un cliente existente. Valida campos y registra la auditoría.</summary>
        public void Editar(Cliente cliente, string usuarioAccion)
        {
            Normalizar(cliente);
            Validar(cliente);

            _datos.Actualizar(cliente);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Editar cliente {cliente.Cedula}");
        }

        /// <summary>Desactiva un cliente (eliminación lógica) y registra la auditoría.</summary>
        public void Eliminar(string cedula, string usuarioAccion)
        {
            _datos.Eliminar(cedula);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Desactivar cliente {cedula}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta los espacios de los campos de texto.</summary>
        private static void Normalizar(Cliente c)
        {
            c.Cedula = (c.Cedula ?? string.Empty).Trim();
            c.Nombre = (c.Nombre ?? string.Empty).Trim();
            c.Telefono = (c.Telefono ?? string.Empty).Trim();
            c.Direccion = (c.Direccion ?? string.Empty).Trim();
            c.Correo = (c.Correo ?? string.Empty).Trim();
            c.Tipo = (c.Tipo ?? string.Empty).Trim();
        }

        /// <summary>Valida los campos obligatorios de un cliente.</summary>
        private static void Validar(Cliente c)
        {
            if (string.IsNullOrWhiteSpace(c.Cedula))
                throw new InvalidOperationException("La cédula del cliente es obligatoria.");
            if (string.IsNullOrWhiteSpace(c.Nombre))
                throw new InvalidOperationException("El nombre del cliente es obligatorio.");
        }

        /// <summary>Detecta violación de clave primaria/única (cédula duplicada).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
