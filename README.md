# Sistema Web de Gestión para el Taller Mecánico Automotriz "JUAN" (SGTMJ)

Aplicación web desarrollada en **ASP.NET Core MVC (.NET 8)** con arquitectura de
**3 capas + Entidades**, usando **ADO.NET (`Microsoft.Data.SqlClient`)** e invocando
**procedimientos almacenados** de SQL Server (base de datos `TallerJuanDB`). No se usa Entity Framework.

## Estructura de la solución

| Proyecto | Tipo | Rol |
|----------|------|-----|
| `TallerJuan.Entidades` | Class Library | Capa de Entidades / Modelo (clases POCO) |
| `TallerJuan.Datos` | Class Library | Capa de Acceso a Datos (DAL) — contiene `ConexionBD` |
| `TallerJuan.Negocio` | Class Library | Capa de Lógica de Negocio (BLL) — contiene `Seguridad` |
| `TallerJuan.Web` | ASP.NET Core MVC | Capa de Presentación (Web) |

Dependencias: `Web` → `Negocio` → `Datos` → `Entidades`.

## Cómo ejecutar

```
dotnet run --project TallerJuan.Web
```

## Estado

Fase 1: estructura base de la solución creada y compilando correctamente.
