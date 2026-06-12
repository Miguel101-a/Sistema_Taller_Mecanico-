using Microsoft.Data.SqlClient;
using TallerJuan.Datos;
using TallerJuan.Entidades;

namespace TallerJuan.Negocio
{
    /// <summary>
    /// Capa de Negocio para Vehículos. Cada vehículo pertenece a un cliente.
    /// Valida, traduce errores de BD y registra la auditoría. La capa Web SIEMPRE pasa por aquí.
    /// </summary>
    public class CN_Vehiculo
    {
        private const string Modulo = "Vehiculos";
        private const int ErrorUniqueIndex = 2601;
        private const int ErrorUniqueConstraint = 2627;

        private readonly CD_Vehiculo _datos = new CD_Vehiculo();
        private readonly CD_Acceso _acceso = new CD_Acceso();

        /// <summary>Devuelve todos los vehículos (con el nombre del cliente).</summary>
        public List<Vehiculo> Listar() => _datos.Listar();

        /// <summary>Devuelve un vehículo por su placa, o null si no existe.</summary>
        public Vehiculo? Obtener(string placa) => _datos.Obtener(placa);

        /// <summary>Devuelve los vehículos ACTIVOS de un cliente (para crear órdenes de trabajo).</summary>
        public List<Vehiculo> ListarPorCliente(string clienteCedula) => _datos.ListarPorCliente(clienteCedula);

        /// <summary>Crea un vehículo nuevo. Valida, traduce la placa duplicada y registra la auditoría.</summary>
        public void Crear(Vehiculo vehiculo, string usuarioAccion)
        {
            Normalizar(vehiculo);
            Validar(vehiculo);

            try
            {
                _datos.Insertar(vehiculo);
                _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Crear vehículo {vehiculo.Placa}");
            }
            catch (SqlException ex) when (EsDuplicado(ex))
            {
                throw new InvalidOperationException("Ya existe un vehículo con esa placa.");
            }
        }

        /// <summary>Actualiza un vehículo existente. Valida y registra la auditoría.</summary>
        public void Editar(Vehiculo vehiculo, string usuarioAccion)
        {
            Normalizar(vehiculo);
            Validar(vehiculo);

            _datos.Actualizar(vehiculo);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Editar vehículo {vehiculo.Placa}");
        }

        /// <summary>Desactiva un vehículo (eliminación lógica) y registra la auditoría.</summary>
        public void Eliminar(string placa, string usuarioAccion)
        {
            _datos.Eliminar(placa);
            _acceso.RegistrarAuditoria(usuarioAccion, Modulo, $"Desactivar vehículo {placa}");
        }

        // ----------------------------------------------------------------------------------
        // Auxiliares
        // ----------------------------------------------------------------------------------

        /// <summary>Recorta los espacios de los campos de texto y normaliza la placa a mayúsculas.</summary>
        private static void Normalizar(Vehiculo v)
        {
            v.Placa = (v.Placa ?? string.Empty).Trim().ToUpperInvariant();
            v.Marca = (v.Marca ?? string.Empty).Trim();
            v.Modelo = (v.Modelo ?? string.Empty).Trim();
            v.Color = (v.Color ?? string.Empty).Trim();
            v.NumeroChasis = (v.NumeroChasis ?? string.Empty).Trim();
            v.NumeroMotor = (v.NumeroMotor ?? string.Empty).Trim();
            v.TipoCombustible = (v.TipoCombustible ?? string.Empty).Trim();
            v.ClienteCedula = (v.ClienteCedula ?? string.Empty).Trim();
        }

        /// <summary>Valida los campos obligatorios y los rangos numéricos de un vehículo.</summary>
        private static void Validar(Vehiculo v)
        {
            if (string.IsNullOrWhiteSpace(v.Placa))
                throw new InvalidOperationException("La placa del vehículo es obligatoria.");
            if (string.IsNullOrWhiteSpace(v.Marca))
                throw new InvalidOperationException("La marca del vehículo es obligatoria.");
            if (string.IsNullOrWhiteSpace(v.Modelo))
                throw new InvalidOperationException("El modelo del vehículo es obligatorio.");
            if (string.IsNullOrWhiteSpace(v.ClienteCedula))
                throw new InvalidOperationException("Debe seleccionar el cliente propietario del vehículo.");

            int anioMaximo = DateTime.Now.Year + 1;
            if (v.Anio.HasValue && (v.Anio < 1950 || v.Anio > anioMaximo))
                throw new InvalidOperationException($"El año debe estar entre 1950 y {anioMaximo}.");

            if (v.Kilometraje.HasValue && v.Kilometraje < 0)
                throw new InvalidOperationException("El kilometraje no puede ser negativo.");
        }

        /// <summary>Detecta violación de clave primaria/única (placa duplicada).</summary>
        private static bool EsDuplicado(SqlException ex) =>
            ex.Number == ErrorUniqueConstraint || ex.Number == ErrorUniqueIndex;
    }
}
