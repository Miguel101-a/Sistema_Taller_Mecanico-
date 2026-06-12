# Sistema Web de Gestión para el Taller Mecánico Automotriz "JUAN" (SGTMJ)

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4) ![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core-MVC-5C2D91) ![SQL Server](https://img.shields.io/badge/SQL%20Server-ADO.NET%20%2B%20SP-CC2927)

Sistema web para la gestión integral del **Taller Mecánico Automotriz "JUAN"**: administra clientes y
vehículos, órdenes de trabajo con su diagnóstico, inventario de repuestos con movimientos de stock,
cotizaciones, facturación y reportes estadísticos, todo bajo un esquema de seguridad con roles y
permisos. Está construido con **ASP.NET Core MVC (.NET 8)** sobre una arquitectura de tres capas más
entidades, usando **ADO.NET con procedimientos almacenados** sobre SQL Server (sin Entity Framework).


## Arquitectura

La solución está dividida en **4 proyectos** con una regla de dependencias estricta y unidireccional:

```
┌─────────────────────┐
│   TallerJuan.Web     │  Presentación: Controladores, Vistas Razor, sesión y permisos
│  (ASP.NET Core MVC)  │
└──────────┬──────────┘
           │ depende de
           ▼
┌─────────────────────┐
│  TallerJuan.Negocio  │  Lógica de negocio (CN_*): validaciones, reglas, auditoría
│   (Class Library)    │
└──────────┬──────────┘
           │ depende de
           ▼
┌─────────────────────┐
│   TallerJuan.Datos   │  Acceso a datos (CD_*): ADO.NET, invocación de SPs, ConexionBD
│   (Class Library)    │
└──────────┬──────────┘
           │ depende de
           ▼
┌─────────────────────┐
│ TallerJuan.Entidades │  Clases POCO del modelo (sin dependencias)
│   (Class Library)    │
└─────────────────────┘
```

| Proyecto | Tipo | Rol |
|----------|------|-----|
| `TallerJuan.Entidades` | Class Library | Capa de Entidades / Modelo (clases POCO) |
| `TallerJuan.Datos` | Class Library | Capa de Acceso a Datos (DAL) — invoca los SPs; contiene `ConexionBD` |
| `TallerJuan.Negocio` | Class Library | Capa de Lógica de Negocio (BLL) — validaciones, auditoría y `Seguridad` (hash) |
| `TallerJuan.Web` | ASP.NET Core MVC | Capa de Presentación (Web) — controladores, vistas y sesión |

**Regla de dependencias:** `Web → Negocio → Datos → Entidades`. Ninguna capa inferior conoce a una
superior; la capa Web nunca accede directamente a Datos, siempre pasa por Negocio.

## Tecnologías

- **C#** sobre **.NET 8**.
- **ASP.NET Core MVC (.NET 8)** para la capa de presentación.
- **SQL Server** como motor de base de datos.
- **ADO.NET** (`Microsoft.Data.SqlClient`) con **procedimientos almacenados** (sin Entity Framework).
- **Bootstrap 5** y **Bootstrap Icons** para la interfaz.
- **Sesiones del servidor** con esquema de **permisos por rol** (relación N:M ROL_PERMISO).
- **SHA-256** para el hash de contraseñas (compatible con SQL Server).

## Base de datos

La base de datos `TallerJuanDB` se compone de **11 tablas** de negocio más las tablas de **seguridad**
(roles, permisos y usuarios/empleados) y la tabla de **auditoría** que registra cada acción de escritura.

Se destacan las **4 tablas N:M** y la relación **1:1** del modelo:

- `ROL_PERMISO` — N:M entre roles y permisos.
- `MOVIMIENTO_INVENTARIO` — N:M entre productos y órdenes (entradas/salidas de stock).
- `DETALLE_COTIZACION` — N:M entre cotizaciones y productos/servicios.
- `DETALLE_FACTURA` — N:M entre facturas y productos/servicios.
- **FACTURA ↔ ORDEN_TRABAJO** — relación **1:1** (una orden genera como máximo una factura, forzada
  por una restricción UNIQUE sobre la clave foránea).

Toda la lógica de inserción, actualización, cálculo de totales (IVA 13 %) y reportes se realiza mediante
**procedimientos almacenados**.

## Instalación y ejecución (paso a paso)

### Requisitos previos

- **Visual Studio 2022** (o superior).
- **.NET 8 SDK**.
- **SQL Server** (Express o superior) y **SQL Server Management Studio (SSMS)**.

### Pasos

1. **Crear la base de datos y los objetos.** En SSMS, ejecutar los scripts de la carpeta `Scripts/`
   **en este orden**:
   1. `Fase0_BaseDatos_TallerJuanDB.sql` — crea la base `TallerJuanDB`, las tablas, la seguridad, la
      auditoría y los datos iniciales (incluido el usuario administrador).
   2. `Fase3_Roles_SPs.sql` — procedimientos de roles y permisos.
   3. `Fase4_Ordenes_SPs.sql` — procedimientos de maestros y órdenes de trabajo.
   4. `Fase5_Inventario_Cotizaciones_SPs.sql` — procedimientos de inventario y cotizaciones.
   5. `Fase6_Facturacion_Reportes_SPs.sql` — procedimientos de facturación y los 5 reportes.

2. **Ajustar la cadena de conexión.** En `TallerJuan.Web/appsettings.json`, editar
   `ConnectionStrings:TallerJuanDB` con el nombre de su servidor SQL Server:

   ```json
   "ConnectionStrings": {
     "TallerJuanDB": "Server=SU_SERVIDOR;Database=TallerJuanDB;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

3. **Ejecutar la aplicación.** Fijar `TallerJuan.Web` como proyecto de inicio en Visual Studio y pulsar
   *Ejecutar*, o bien desde la terminal:

   ```bash
   dotnet run --project TallerJuan.Web
   ```

4. **Iniciar sesión** con las credenciales iniciales del administrador:

   - **Usuario:** `juan`
   - **Contraseña:** `juan`

## Módulos del sistema

1. **Inicio (Dashboard)** — tarjetas resumen del taller según los permisos del usuario.
2. **Clientes** — alta, edición y baja lógica de clientes.
3. **Vehículos** — registro de vehículos asociados a cada cliente.
4. **Órdenes de Trabajo** — recepción, diagnóstico y seguimiento de estados hasta la entrega.
5. **Inventario y Repuestos** — productos, stock, movimientos de entrada/salida y alerta de stock mínimo.
6. **Empleados** — gestión del personal del taller (incluidos los mecánicos).
7. **Cotizaciones** — presupuestos con detalle de servicios y repuestos e IVA 13 %.
8. **Facturación** — facturas a partir de órdenes finalizadas, con emisión y anulación.
9. **Reportes y Estadísticas** — los 5 reportes del ERS (ingresos, servicios, productividad, repuestos y
   clientes frecuentes).
10. **Seguridad** — gestión de roles y asignación de permisos (ROL_PERMISO).

## Metodología

El sistema se desarrolló de forma **incremental por fases**, siguiendo un enfoque tipo **Scrum**: cada
fase corresponde a una rama de trabajo y a un **Pull Request** independiente hacia `main`
(**PR #1 a PR #7**), lo que mantiene un historial claro y revisable de la evolución del proyecto.

## Autor

**José Miguel Valencia Apaza**
Ingeniería de Sistemas — UDABOL.
Asignatura: Taller de Sistemas (I/2026).
