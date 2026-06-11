-- ============================================================
-- FASE 5 - SPs de Movimientos de Inventario y Cotizaciones
-- Motor: SQL Server 2012+
-- Ejecutar UNA sola vez sobre TallerJuanDB
-- ============================================================
USE TallerJuanDB;
GO

-- ==================== MOVIMIENTOS DE INVENTARIO (N:M PRODUCTO <-> ORDEN_TRABAJO) ====================

IF OBJECT_ID('sp_MovInventario_Listar','P') IS NOT NULL DROP PROCEDURE sp_MovInventario_Listar;
GO
CREATE PROCEDURE sp_MovInventario_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT M.ID_MOVIMIENTO, M.PRODUCTO_CODIGO, P.NOMBRE AS PRODUCTO_NOMBRE,
           M.ORDEN_TRABAJO_NUMERO_ORDEN, M.TIPO, M.FECHA, M.MOTIVO, M.CANTIDAD,
           P.STOCK_ACTUAL
    FROM MOVIMIENTO_INVENTARIO M
    INNER JOIN PRODUCTO P ON M.PRODUCTO_CODIGO = P.CODIGO
    ORDER BY M.FECHA DESC;
END
GO

IF OBJECT_ID('sp_MovInventario_PorProducto','P') IS NOT NULL DROP PROCEDURE sp_MovInventario_PorProducto;
GO
CREATE PROCEDURE sp_MovInventario_PorProducto
    @ProductoCodigo VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT M.ID_MOVIMIENTO, M.PRODUCTO_CODIGO, P.NOMBRE AS PRODUCTO_NOMBRE,
           M.ORDEN_TRABAJO_NUMERO_ORDEN, M.TIPO, M.FECHA, M.MOTIVO, M.CANTIDAD,
           P.STOCK_ACTUAL
    FROM MOVIMIENTO_INVENTARIO M
    INNER JOIN PRODUCTO P ON M.PRODUCTO_CODIGO = P.CODIGO
    WHERE M.PRODUCTO_CODIGO = @ProductoCodigo
    ORDER BY M.FECHA DESC;
END
GO

-- Inserta un movimiento y actualiza el stock EN LA MISMA TRANSACCION.
-- SALIDA: valida stock suficiente y descuenta. INGRESO: aumenta.
IF OBJECT_ID('sp_MovInventario_Insertar','P') IS NOT NULL DROP PROCEDURE sp_MovInventario_Insertar;
GO
CREATE PROCEDURE sp_MovInventario_Insertar
    @ProductoCodigo VARCHAR(20),
    @NumeroOrden    INT,
    @Tipo           VARCHAR(20),   -- 'INGRESO' o 'SALIDA'
    @Motivo         VARCHAR(200),
    @Cantidad       INT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        BEGIN TRANSACTION;

        IF @Cantidad <= 0
        BEGIN
            RAISERROR('La cantidad debe ser mayor a cero.', 16, 1);
        END

        IF @Tipo = 'SALIDA'
        BEGIN
            DECLARE @StockActual INT;
            SELECT @StockActual = STOCK_ACTUAL FROM PRODUCTO WITH (UPDLOCK) WHERE CODIGO = @ProductoCodigo;

            IF @StockActual IS NULL
                RAISERROR('El producto no existe.', 16, 1);

            IF @StockActual < @Cantidad
                RAISERROR('Stock insuficiente para realizar la salida.', 16, 1);

            UPDATE PRODUCTO SET STOCK_ACTUAL = STOCK_ACTUAL - @Cantidad WHERE CODIGO = @ProductoCodigo;
        END
        ELSE IF @Tipo = 'INGRESO'
        BEGIN
            UPDATE PRODUCTO SET STOCK_ACTUAL = STOCK_ACTUAL + @Cantidad WHERE CODIGO = @ProductoCodigo;
        END
        ELSE
        BEGIN
            RAISERROR('Tipo de movimiento invalido. Use INGRESO o SALIDA.', 16, 1);
        END

        INSERT INTO MOVIMIENTO_INVENTARIO (PRODUCTO_CODIGO, ORDEN_TRABAJO_NUMERO_ORDEN, TIPO, MOTIVO, CANTIDAD)
        VALUES (@ProductoCodigo, @NumeroOrden, @Tipo, @Motivo, @Cantidad);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;  -- relanza el error para que lo capture C#
    END CATCH
END
GO

-- ==================== COTIZACIONES (cabecera) ====================

IF OBJECT_ID('sp_Cotizacion_Listar','P') IS NOT NULL DROP PROCEDURE sp_Cotizacion_Listar;
GO
CREATE PROCEDURE sp_Cotizacion_Listar
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Q.NUMERO_COTIZACION, Q.FECHA_EMISION, Q.VALIDEZ_DIAS,
           Q.SUBTOTAL_SERVICIOS, Q.SUBTOTAL_REPUESTOS, Q.IMPUESTOS, Q.TOTAL, Q.ESTADO,
           Q.CLIENTE_CEDULA, C.NOMBRE AS CLIENTE_NOMBRE,
           Q.VEHICULO_PLACA, V.MARCA + ' ' + V.MODELO AS VEHICULO_DESCRIPCION
    FROM COTIZACION Q
    INNER JOIN CLIENTE  C ON Q.CLIENTE_CEDULA = C.CEDULA
    INNER JOIN VEHICULO V ON Q.VEHICULO_PLACA = V.PLACA
    ORDER BY Q.NUMERO_COTIZACION DESC;
END
GO

IF OBJECT_ID('sp_Cotizacion_Obtener','P') IS NOT NULL DROP PROCEDURE sp_Cotizacion_Obtener;
GO
CREATE PROCEDURE sp_Cotizacion_Obtener
    @NumeroCotizacion INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Q.NUMERO_COTIZACION, Q.FECHA_EMISION, Q.VALIDEZ_DIAS,
           Q.SUBTOTAL_SERVICIOS, Q.SUBTOTAL_REPUESTOS, Q.IMPUESTOS, Q.TOTAL, Q.ESTADO,
           Q.CLIENTE_CEDULA, C.NOMBRE AS CLIENTE_NOMBRE,
           Q.VEHICULO_PLACA, V.MARCA + ' ' + V.MODELO AS VEHICULO_DESCRIPCION
    FROM COTIZACION Q
    INNER JOIN CLIENTE  C ON Q.CLIENTE_CEDULA = C.CEDULA
    INNER JOIN VEHICULO V ON Q.VEHICULO_PLACA = V.PLACA
    WHERE Q.NUMERO_COTIZACION = @NumeroCotizacion;
END
GO

IF OBJECT_ID('sp_Cotizacion_Insertar','P') IS NOT NULL DROP PROCEDURE sp_Cotizacion_Insertar;
GO
CREATE PROCEDURE sp_Cotizacion_Insertar
    @ValidezDias    INT,
    @ClienteCedula  VARCHAR(20),
    @VehiculoPlaca  VARCHAR(10)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO COTIZACION (VALIDEZ_DIAS, CLIENTE_CEDULA, VEHICULO_PLACA)
    VALUES (@ValidezDias, @ClienteCedula, @VehiculoPlaca);
    SELECT CAST(SCOPE_IDENTITY() AS INT) AS NUMERO_COTIZACION;
END
GO

IF OBJECT_ID('sp_Cotizacion_CambiarEstado','P') IS NOT NULL DROP PROCEDURE sp_Cotizacion_CambiarEstado;
GO
CREATE PROCEDURE sp_Cotizacion_CambiarEstado
    @NumeroCotizacion INT,
    @Estado           VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE COTIZACION SET ESTADO = @Estado WHERE NUMERO_COTIZACION = @NumeroCotizacion;
END
GO

-- Recalcula subtotales (por TIPO de linea), IVA 13% y total de una cotizacion.
IF OBJECT_ID('sp_Cotizacion_RecalcularTotales','P') IS NOT NULL DROP PROCEDURE sp_Cotizacion_RecalcularTotales;
GO
CREATE PROCEDURE sp_Cotizacion_RecalcularTotales
    @NumeroCotizacion INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Servicios DECIMAL(10,2), @Repuestos DECIMAL(10,2), @Impuestos DECIMAL(10,2);

    SELECT @Servicios = ISNULL(SUM(CASE WHEN TIPO = 'SERVICIO' THEN SUBTOTAL ELSE 0 END), 0),
           @Repuestos = ISNULL(SUM(CASE WHEN TIPO = 'REPUESTO' THEN SUBTOTAL ELSE 0 END), 0)
    FROM DETALLE_COTIZACION
    WHERE COTIZACION_NUMERO_COTIZACION = @NumeroCotizacion;

    SET @Impuestos = ROUND((@Servicios + @Repuestos) * 0.13, 2);  -- IVA 13% Bolivia

    UPDATE COTIZACION
    SET SUBTOTAL_SERVICIOS = @Servicios,
        SUBTOTAL_REPUESTOS = @Repuestos,
        IMPUESTOS = @Impuestos,
        TOTAL = @Servicios + @Repuestos + @Impuestos
    WHERE NUMERO_COTIZACION = @NumeroCotizacion;
END
GO

-- ==================== DETALLE DE COTIZACION (N:M COTIZACION <-> PRODUCTO) ====================

IF OBJECT_ID('sp_DetalleCotizacion_PorCotizacion','P') IS NOT NULL DROP PROCEDURE sp_DetalleCotizacion_PorCotizacion;
GO
CREATE PROCEDURE sp_DetalleCotizacion_PorCotizacion
    @NumeroCotizacion INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT D.COTIZACION_NUMERO_COTIZACION, D.PRODUCTO_CODIGO, P.NOMBRE AS PRODUCTO_NOMBRE,
           D.ID_DETALLE_COTIZACION, D.DESCRIPCION, D.TIPO, D.CANTIDAD, D.PRECIO_UNITARIO, D.SUBTOTAL
    FROM DETALLE_COTIZACION D
    INNER JOIN PRODUCTO P ON D.PRODUCTO_CODIGO = P.CODIGO
    WHERE D.COTIZACION_NUMERO_COTIZACION = @NumeroCotizacion
    ORDER BY D.ID_DETALLE_COTIZACION;
END
GO

-- Inserta una linea; el SUBTOTAL se calcula aqui (CANTIDAD * PRECIO_UNITARIO).
-- La PK compuesta impide repetir el mismo producto en la cotizacion (error 2627 capturado en C#).
IF OBJECT_ID('sp_DetalleCotizacion_Insertar','P') IS NOT NULL DROP PROCEDURE sp_DetalleCotizacion_Insertar;
GO
CREATE PROCEDURE sp_DetalleCotizacion_Insertar
    @NumeroCotizacion INT,
    @ProductoCodigo   VARCHAR(20),
    @Descripcion      VARCHAR(200),
    @Tipo             VARCHAR(20),   -- 'SERVICIO' o 'REPUESTO'
    @Cantidad         INT,
    @PrecioUnitario   DECIMAL(10,2)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO DETALLE_COTIZACION
        (COTIZACION_NUMERO_COTIZACION, PRODUCTO_CODIGO, DESCRIPCION, TIPO, CANTIDAD, PRECIO_UNITARIO, SUBTOTAL)
    VALUES
        (@NumeroCotizacion, @ProductoCodigo, @Descripcion, @Tipo, @Cantidad, @PrecioUnitario,
         @Cantidad * @PrecioUnitario);
END
GO

IF OBJECT_ID('sp_DetalleCotizacion_Eliminar','P') IS NOT NULL DROP PROCEDURE sp_DetalleCotizacion_Eliminar;
GO
CREATE PROCEDURE sp_DetalleCotizacion_Eliminar
    @NumeroCotizacion INT,
    @ProductoCodigo   VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM DETALLE_COTIZACION
    WHERE COTIZACION_NUMERO_COTIZACION = @NumeroCotizacion
      AND PRODUCTO_CODIGO = @ProductoCodigo;
END
GO

PRINT 'Fase 5: SPs de Movimientos de Inventario y Cotizaciones creados correctamente.';
GO
