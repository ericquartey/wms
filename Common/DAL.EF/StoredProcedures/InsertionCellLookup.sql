
CREATE OR ALTER PROCEDURE [dbo].[InsertionCellLookup]
(
	@ItemId INT,
	@LoadingUnitTypeId INT,
	@AllowDeclass BIT = 1,
	@OutputAisleId INT OUTPUT,
	@OutputCellId INT OUTPUT,
	@OutputCellPositionId INT OUTPUT,
	@OutputErrorMessage NVARCHAR(255) OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ItemClass CHAR(1),
			@AisleId INT,
			@CellTypeId INT

	-- Fetch current item class
	SELECT @ItemClass = Class FROM Items WHERE Id=@ItemId
	--PRINT 'Item class: ' + @ItemClass

	-- Find compatible CellTypes
	DECLARE CompatibleCellTypesCursor CURSOR FOR
		SELECT
			CellConfigurationCellTypes.CellTypeId
		FROM CellConfigurationCellPositionLoadingUnitTypes
		JOIN CellConfigurations ON CellConfigurations.Id=CellConfigurationCellPositionLoadingUnitTypes.CellConfigurationId
		JOIN CellConfigurationCellTypes ON CellConfigurationCellTypes.CellConfigurationId=CellConfigurations.Id
		JOIN CellTypes ON CellTypes.Id=CellConfigurationCellTypes.CellTypeId
		WHERE CellConfigurationCellPositionLoadingUnitTypes.LoadingUnitTypeId=@LoadingUnitTypeId
		ORDER BY CellConfigurationCellTypes.Priority, CellConfigurationCellPositionLoadingUnitTypes.Priority

	-- Define priority order of aisles, based on item class
	IF @ItemClass IN ('A', 'B')
	BEGIN
		-- Stock of the item in each aisle should be as balanced as possible
		DECLARE AislesCursor CURSOR FOR
			SELECT
				Aisles.Id AS AisleId
			FROM Aisles
				LEFT JOIN Cells ON Cells.AisleId=Aisles.Id
				LEFT JOIN LoadingUnits ON LoadingUnits.CellId=Cells.Id
				LEFT JOIN Compartments ON Compartments.LoadingUnitId=LoadingUnits.Id AND Compartments.ItemId=@ItemId
			WHERE Aisles.Id IN (
				SELECT AisleId FROM LoadingUnitTypesAisles WHERE LoadingUnitTypeId=@LoadingUnitTypeId
			)
			AND Aisles.AreaId IN (
				SELECT ItemsAreas.AreaId FROM ItemsAreas WHERE ItemsAreas.ItemId=@ItemId
			) 
			GROUP BY Aisles.Id
			ORDER BY
				COALESCE(SUM(COALESCE(Compartments.Stock, 0) - COALESCE(Compartments.ReservedForPick, 0) + COALESCE(Compartments.ReservedToStore, 0)), 0),
				SUM(CASE WHEN LoadingUnits.Id IS NOT NULL THEN 1 ELSE 0 END),
				AisleId;
	END
	ELSE
	BEGIN
		-- Number of busy C class cells in each aisle should be as balanced as possible
		DECLARE AislesCursor CURSOR FOR
			SELECT
				Aisles.Id AS AisleId
			FROM Aisles
				LEFT JOIN Cells ON Cells.AisleId=Aisles.Id
				LEFT JOIN LoadingUnits ON LoadingUnits.CellId=Cells.Id
			WHERE Aisles.Id IN (
				SELECT AisleId FROM LoadingUnitTypesAisles WHERE LoadingUnitTypeId=@LoadingUnitTypeId
			)
			AND Aisles.AreaId IN (
				SELECT ItemsAreas.AreaId FROM ItemsAreas WHERE ItemsAreas.ItemId=@ItemId
			)
			GROUP BY Aisles.Id
			ORDER BY
				SUM(CASE WHEN LoadingUnits.Id IS NOT NULL AND Cells.Class=@ItemClass THEN 1 ELSE 0 END),
				SUM(CASE WHEN LoadingUnits.Id IS NOT NULL THEN 1 ELSE 0 END),
				AisleId
	END

Look_for_position:
	
	OPEN AislesCursor
	FETCH NEXT FROM AislesCursor INTO @AisleId
	WHILE @@FETCH_STATUS = 0
	BEGIN
		PRINT 'Search for available cells in AisleId: ' + CONVERT(VARCHAR, @AisleId)

		OPEN CompatibleCellTypesCursor
		FETCH NEXT FROM CompatibleCellTypesCursor INTO @CellTypeId
		WHILE @@FETCH_STATUS = 0
		BEGIN
			PRINT '- Looking for CellTypeId: ' + CONVERT(VARCHAR, @CellTypeId)

			-- Available positions
			SELECT TOP 1
			  @OutputAisleId = C.AisleId,
			  @OutputCellId = C.Id,
			  @OutputCellPositionId = CCCPLUT_Available.CellPositionId
			FROM Cells C
			  LEFT JOIN LoadingUnits LU ON C.Id=LU.CellId
			  LEFT JOIN CellConfigurationCellPositionLoadingUnitTypes CCCPLUT_Busy
				ON CCCPLUT_Busy.CellPositionId=LU.CellPositionId 
				   AND LU.LoadingUnitTypeId=CCCPLUT_Busy.LoadingUnitTypeId
			  JOIN CellTypes CT ON C.CellTypeId = CT.Id
			  JOIN CellConfigurationCellTypes CCCT ON CT.Id = CCCT.CellTypeId
			  JOIN CellConfigurations CC ON CCCT.CellConfigurationId = CC.Id
			  JOIN CellConfigurationCellPositionLoadingUnitTypes CCCPLUT_Available on CC.Id = CCCPLUT_Available.CellConfigurationId AND CCCPLUT_Available.LoadingUnitTypeId=@LoadingUnitTypeId
			WHERE C.AisleId=@AisleId
				  AND C.Class=@ItemClass
				  AND C.CellTypeId=@CellTypeId
				  AND (CCCPLUT_Busy.CellConfigurationId IS NULL OR CCCPLUT_Busy.CellConfigurationId=CC.Id) -- same configuration
				  AND (LU.CellPositionId IS NULL OR LU.CellPositionId<>CCCPLUT_Available.CellPositionId) -- different position
			ORDER BY
			  C.Priority,
			  CCCT.Priority,
			  CCCPLUT_Available.Priority
			
			IF @@ROWCOUNT = 0
			BEGIN
				PRINT '-- Not found in this CellTypeId... trying next!'
				FETCH NEXT FROM CompatibleCellTypesCursor INTO @CellTypeId
			END
			ELSE
			BEGIN
				PRINT '-- Found CellId: ' + CONVERT(VARCHAR, @OutputCellId) + ', CellPositionId: ' + CONVERT(VARCHAR, @OutputCellPositionId)
				
				CLOSE CompatibleCellTypesCursor
				CLOSE AislesCursor

				DEALLOCATE CompatibleCellTypesCursor
				DEALLOCATE AislesCursor

				RETURN 0
			END
		END
		CLOSE CompatibleCellTypesCursor

		PRINT '- Not found in this AisleId... trying next!'
		FETCH NEXT FROM AislesCursor INTO @AisleId
	END
	CLOSE AislesCursor

	-- manage item declassification
	IF @AllowDeclass=1 AND @ItemClass<>'C'
	BEGIN 
		IF @ItemClass='A'
		BEGIN 
			SET @ItemClass='B'
		END
		ELSE
		BEGIN
			IF @ItemClass='B'
			BEGIN
				SET @ItemClass='C'
			END
		END
		PRINT '-- Item declassed to class: ' + @ItemClass 
		GOTO Look_for_position
	END
	
	DEALLOCATE CompatibleCellTypesCursor
	DEALLOCATE AislesCursor

	PRINT 'Not found at all!'
	SET @OutputErrorMessage = 'Cell not found'
	RETURN -1
END
GO




/*
DELETE FROM dbo.Compartments;
DELETE FROM LoadingUnits;
*/



/*
DECLARE @ItemId INT = 3,
        @ItemQuantity INT = 1,
        @LoadingUnitTypeId INT = 1,
        @AllowDeclass BIT = 0,
        @ExecResult INT,
        @OutputAisleId INT, 
        @OutputCellId INT, 
        @OutputCellPositionId INT, 
        @OutputErrorMessage NVARCHAR(255),
        @ItemClass CHAR(1),
        @LoadingUnitId INT,
        @CompartmentId INT;

EXEC @ExecResult = InsertionCellLookup @ItemId=@ItemId, 
                                       @LoadingUnitTypeId=@LoadingUnitTypeId,
                                       @AllowDeclass=@AllowDeclass, 
                                       @OutputAisleId=@OutputAisleId OUTPUT, 
                                       @OutputCellId=@OutputCellId OUTPUT, 
                                       @OutputCellPositionId=@OutputCellPositionId OUTPUT, 
                                       @OutputErrorMessage=@OutputErrorMessage OUTPUT;
IF @ExecResult<>0
BEGIN
	PRINT 'ErrorMessage: ' + CONVERT(VARCHAR, @OutputErrorMessage);
	RETURN
END
ELSE
BEGIN
	PRINT 'AisleId: ' + CONVERT(VARCHAR, @OutputAisleId);
	PRINT 'CellId: ' + CONVERT(VARCHAR, @OutputCellId);
	PRINT 'CellPositionId: ' + CONVERT(VARCHAR, @OutputCellPositionId);

	SELECT @ItemClass = Class FROM Items WHERE Id=@ItemId;

	INSERT INTO LoadingUnits (Code, CellId, CellPairing, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, Class)
	VALUES ('', @OutputCellId, 1, @OutputCellPositionId, @LoadingUnitTypeId, 1600, 900, 3, 'M', @ItemClass);
	SET @LoadingUnitId = @@IDENTITY;
	UPDATE LoadingUnits SET Code='UDC' + CONVERT(VARCHAR, @LoadingUnitId) WHERE Id=@LoadingUnitId;

	INSERT INTO Compartments (Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock)
	VALUES ('', @@IDENTITY, 1, 1, @ItemId, 1, 1, 2, @ItemQuantity);
	SET @CompartmentId = @@IDENTITY;
	UPDATE Compartments SET Code='UDC' + CONVERT(VARCHAR, @LoadingUnitId) + '_COMP' + CONVERT(VARCHAR, @CompartmentId) WHERE Id=@CompartmentId;
END
*/