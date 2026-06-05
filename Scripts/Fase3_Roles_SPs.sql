-- ============================================================
-- FASE 3 - SPs para gestion de Roles (CRUD + listado completo)
-- Motor: SQL Server 2012+
-- Ejecutar UNA sola vez sobre TallerJuanDB
-- ============================================================
USE TallerJuanDB;
GO

-- Listar TODOS los roles (ACTIVO e INACTIVO) para el panel de Seguridad.
-- Se mantiene sp_ListarRoles (solo ACTIVO) para combos/dropdowns.
IF OBJECT_ID('sp_Rol_ListarTodos','P') IS NOT NULL DROP PROCEDURE sp_Rol_ListarTodos;
GO
CREATE PROCEDURE sp_Rol_ListarTodos
AS
BEGIN
    SET NOCOUNT ON;
    SELECT ID_ROL, NOMBRE, ESTADO FROM ROL ORDER BY NOMBRE;
END
GO

-- Insertar un rol nuevo (estado ACTIVO por defecto). Devuelve el ID generado.
IF OBJECT_ID('sp_Rol_Insertar','P') IS NOT NULL DROP PROCEDURE sp_Rol_Insertar;
GO
CREATE PROCEDURE sp_Rol_Insertar
    @Nombre VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO ROL (NOMBRE, ESTADO) VALUES (@Nombre, 'ACTIVO');
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS ID_ROL;
END
GO

-- Renombrar un rol.
IF OBJECT_ID('sp_Rol_Actualizar','P') IS NOT NULL DROP PROCEDURE sp_Rol_Actualizar;
GO
CREATE PROCEDURE sp_Rol_Actualizar
    @IdRol  INT,
    @Nombre VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ROL SET NOMBRE = @Nombre WHERE ID_ROL = @IdRol;
END
GO

-- Soft-delete: cambia ESTADO a 'ACTIVO' o 'INACTIVO'.
-- NO se hace borrado fisico por integridad referencial con EMPLEADO y ROL_PERMISO.
IF OBJECT_ID('sp_Rol_CambiarEstado','P') IS NOT NULL DROP PROCEDURE sp_Rol_CambiarEstado;
GO
CREATE PROCEDURE sp_Rol_CambiarEstado
    @IdRol  INT,
    @Estado VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE ROL SET ESTADO = @Estado WHERE ID_ROL = @IdRol;
END
GO

PRINT 'Fase 3: SPs de Rol creados correctamente.';
GO
