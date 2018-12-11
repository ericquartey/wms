SET QUOTED_IDENTIFIER ON

-- clean all tables
EXEC sp_MSforeachtable 'ALTER TABLE ? DISABLE TRIGGER ALL'
EXEC sp_MSforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL'

DECLARE @tablename nvarchar(max)

DECLARE cur CURSOR FOR
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME NOT IN ('__EFMigrationsHistory')
OPEN cur

FETCH cur into @tablename
WHILE @@FETCH_STATUS = 0
BEGIN
	EXEC('DELETE FROM ' + @tablename)

	IF EXISTS (Select * from sys.identity_columns where object_name(object_id) = @tablename)
	BEGIN
		EXEC('DBCC CHECKIDENT (''' + @tablename + ''', RESEED, 0) WITH NO_INFOMSGS')
	END

    FETCH cur into @tablename
END

CLOSE cur
DEALLOCATE cur

EXEC sp_MSforeachtable 'ALTER TABLE ? ENABLE TRIGGER ALL'
EXEC sp_MSforeachtable 'ALTER TABLE ? CHECK CONSTRAINT ALL'


BEGIN TRANSACTION;

-- Areas / Aisles
DECLARE
    @vrtmag_area int = 2,
    @traslo_area int = 1

SET IDENTITY_INSERT Areas ON;
INSERT INTO Areas (Id, Name) VALUES (@traslo_area, 'Traslo Area');
INSERT INTO Areas (Id, Name) VALUES (@vrtmag_area, 'Vertimag Area');
SET IDENTITY_INSERT Areas OFF;

SET IDENTITY_INSERT Aisles ON;
INSERT INTO Aisles (Id, Name, AreaId, Floors, Columns) VALUES (1, 'Aisle 1', @traslo_area, 5, 10);
INSERT INTO Aisles (Id, Name, AreaId, Floors, Columns) VALUES (2, 'Aisle 2', @traslo_area, 5, 10);
INSERT INTO Aisles (Id, Name, AreaId, Floors, Columns) VALUES (3, 'Aisle 3', @traslo_area, 5, 10);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (4, 'Vertimag 1', @vrtmag_area);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (5, 'Vertimag 2', @vrtmag_area);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (6, 'Vertimag 3', @vrtmag_area);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (7, 'Vertimag 4', @vrtmag_area);
SET IDENTITY_INSERT Aisles OFF;

-- Item management types
DECLARE
    @item_management_fifo char(1) = 'F',
    @item_management_vol  char(1) = 'V'

--Items
INSERT INTO AbcClasses (Id, Description) VALUES ('A', 'A Class');
INSERT INTO AbcClasses (Id, Description) VALUES ('B', 'B Class');
INSERT INTO AbcClasses (Id, Description) VALUES ('C', 'C Class');

SET IDENTITY_INSERT ItemCategories ON;
INSERT INTO ItemCategories (Id, Description) VALUES (1, 'Viti');
INSERT INTO ItemCategories (Id, Description) VALUES (2, 'Bulloni');
INSERT INTO ItemCategories (Id, Description) VALUES (3, 'Rondelle');
INSERT INTO ItemCategories (Id, Description) VALUES (4, 'Attrezzi');
INSERT INTO ItemCategories (Id, Description) VALUES (5, 'Cavi');
SET IDENTITY_INSERT ItemCategories OFF;

INSERT INTO MeasureUnits (Id, Description) VALUES ('PZ', 'Pieces');
INSERT INTO MeasureUnits (Id, Description) VALUES ('KG', 'Kilograms');
INSERT INTO MeasureUnits (Id, Description) VALUES ('L' , 'Liters');

SET IDENTITY_INSERT Items ON;
INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId, Image, InventoryDate, LastModificationDate, LastPickDate, LastStoreDate)
  VALUES (1, '0U000498', '000498        FRESA SMUSSO PUNTA KABA'   , 'A', 'PZ', @item_management_fifo, 1, 'Articolo1.jpg', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId, Image, InventoryDate, LastModificationDate, LastPickDate, LastStoreDate)
  VALUES (2, '0U000499', '000499        FRESA SMUSSO PUNTA KESO'   , 'A', 'PZ', @item_management_fifo, 2, 'Articolo2.jpg', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId, Image, InventoryDate, LastModificationDate, LastPickDate, LastStoreDate)
  VALUES (3, '0U000524', '000524        FRESA DESTRA 50X50X22 Z=12', 'B', 'PZ', @item_management_vol , 3, 'Articolo3.jpg', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId, Image, InventoryDate, LastModificationDate, LastPickDate, LastStoreDate)
  VALUES (4, '0U000578', '000578        FRESA DORSI VAC91'         , 'B', 'PZ', @item_management_fifo, 4, 'Articolo4.jpg', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId, Image, InventoryDate, LastModificationDate, LastPickDate, LastStoreDate)
  VALUES (5, '0U000585', '000585        FR.PROF.COSTANTE FR.LAT.'  , 'C', 'PZ', @item_management_vol , 5, 'Articolo5.jpg', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId, Image, InventoryDate, LastModificationDate, LastPickDate, LastStoreDate)
  VALUES (6, '0U000640', '000640        FRESA A PLACCHE RIPORTATE' , 'C', 'PZ', @item_management_fifo, 1, 'Articolo6.jpg', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
SET IDENTITY_INSERT Items OFF;

INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (1, @traslo_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (2, @traslo_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (3, @traslo_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (4, @traslo_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (5, @traslo_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (6, @traslo_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (1, @vrtmag_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (2, @vrtmag_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (3, @vrtmag_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (4, @vrtmag_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (5, @vrtmag_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (6, @vrtmag_area);


-- Cells
SET IDENTITY_INSERT CellStatuses ON;
INSERT INTO CellStatuses (Id, Description) VALUES ( 1, 'Empty');
INSERT INTO CellStatuses (Id, Description) VALUES ( 2, 'Reserved');
INSERT INTO CellStatuses (Id, Description) VALUES ( 3, 'Full');
INSERT INTO CellStatuses (Id, Description) VALUES ( 4, 'Full over Full');
INSERT INTO CellStatuses (Id, Description) VALUES ( 5, 'Empty over Empty');
INSERT INTO CellStatuses (Id, Description) VALUES ( 6, 'Halved');
INSERT INTO CellStatuses (Id, Description) VALUES ( 7, 'Unusable');
INSERT INTO CellStatuses (Id, Description) VALUES (99, 'Disabled');
SET IDENTITY_INSERT CellStatuses OFF;

SET IDENTITY_INSERT CellHeightClasses ON;
INSERT INTO CellHeightClasses (Id, Description, MinHeight, MaxHeight) VALUES (1, 'Cell 1300mm height max', 0, 1300);
INSERT INTO CellHeightClasses (Id, Description, MinHeight, MaxHeight) VALUES (2, 'Cell 1700mm height max', 1300, 1700);
INSERT INTO CellHeightClasses (Id, Description, MinHeight, MaxHeight) VALUES (3, 'Cell 900mm height max - Vertimag', 0, 900);
SET IDENTITY_INSERT CellHeightClasses OFF;

SET IDENTITY_INSERT CellSizeClasses ON;
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 1, 'Europallet'         , 800, 1200);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 2, 'Vertimag tray 65XS' , 1950, 650);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 3, 'Vertimag tray 84XS' , 1950, 840);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 4, 'Vertimag tray 103XS', 1950, 1030);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 5, 'Vertimag tray 65S'  , 2450, 650);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 6, 'Vertimag tray 84S', 2450, 840);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 7, 'Vertimag tray 103S', 2450, 1030);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 8, 'Vertimag tray 65M', 3050, 650);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES ( 9, 'Vertimag tray 84M', 3050, 840);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (10, 'Vertimag tray 103M', 3050, 1030);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (11, 'Vertimag tray 65L', 3650, 650);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (12, 'Vertimag tray 84L', 3650, 840);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (13, 'Vertimag tray 103L', 3650, 1030);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (14, 'Vertimag tray 65XL', 4250, 650);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (15, 'Vertimag tray 84XL', 4250, 840);
INSERT INTO CellSizeClasses (Id, Description, Width, Length) VALUES (16, 'Vertimag tray 103XL', 4250, 1030);
SET IDENTITY_INSERT CellSizeClasses OFF;

SET IDENTITY_INSERT CellWeightClasses ON;
INSERT INTO CellWeightClasses (Id, Description, MinWeight, MaxWeight) VALUES (1, 'Cell 1000kg weight max'          , 0, 1000);
INSERT INTO CellWeightClasses (Id, Description, MinWeight, MaxWeight) VALUES (2, 'Cell 500kg weight max - Vertimag', 0,  500);
SET IDENTITY_INSERT CellWeightClasses OFF;

SET IDENTITY_INSERT CellTypes ON;
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 1, 2, 1,  1,  'Cell Europallet, 1700mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 2, 1, 1,  1,  'Cell Europallet, 1300mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 3, 3, 2,  2,  'Vertimag tray 65XS, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 4, 3, 2,  3,  'Vertimag tray 84XS, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 5, 3, 2,  4,  'Vertimag tray 103XS, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 6, 3, 2,  5,  'Vertimag tray 65S, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 7, 3, 2,  6,  'Vertimag tray 84S, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 8, 3, 2,  7,  'Vertimag tray 103S, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES ( 9, 3, 2,  8,  'Vertimag tray 65M, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (10, 3, 2,  9,  'Vertimag tray 84M, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (11, 3, 2, 10, 'Vertimag tray 103M, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (12, 3, 2, 11, 'Vertimag tray 65L, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (13, 3, 2, 12, 'Vertimag tray 84L, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (14, 3, 2, 13, 'Vertimag tray 103L, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (15, 3, 2, 14, 'Vertimag tray 65XL, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (16, 3, 2, 15, 'Vertimag tray 84XL, 900mm height max, 1000kg weight max');
INSERT INTO CellTypes (Id, CellHeightClassId, CellWeightClassId, CellSizeClassId, Description) VALUES (17, 3, 2, 16, 'Vertimag tray 103XL, 900mm height max, 1000kg weight max');
SET IDENTITY_INSERT CellTypes OFF;

SET IDENTITY_INSERT Cells ON;
-- automatic warehouse
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (1, 1, 1, 1, 'L', 1, 1, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (2, 1, 1, 1, 'R', 2, 2, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (3, 1, 1, 2, 'L', 3, 3, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (4, 1, 1, 2, 'R', 4, 4, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (5, 1, 1, 3, 'L', 5, 5, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (6, 1, 1, 3, 'R', 6, 6, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (7, 1, 1, 4, 'L', 7, 7, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (8, 1, 1, 4, 'R', 8, 8, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (9, 1, 1, 5, 'L', 9, 9, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (10, 1, 1, 5, 'R', 10, 10, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (11, 1, 1, 6, 'L', 11, 11, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (12, 1, 1, 6, 'R', 12, 12, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (13, 1, 1, 7, 'L', 13, 13, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (14, 1, 1, 7, 'R', 14, 14, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (15, 1, 1, 8, 'L', 15, 15, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (16, 1, 1, 8, 'R', 16, 16, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (17, 1, 1, 9, 'L', 17, 17, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (18, 1, 1, 9, 'R', 18, 18, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (19, 1, 1, 10, 'L', 19, 19, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (20, 1, 1, 10, 'R', 20, 20, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (21, 1, 2, 1, 'L', 21, 21, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (22, 1, 2, 1, 'R', 22, 22, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (23, 1, 2, 2, 'L', 23, 23, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (24, 1, 2, 2, 'R', 24, 24, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (25, 1, 2, 3, 'L', 25, 25, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (26, 1, 2, 3, 'R', 26, 26, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (27, 1, 2, 4, 'L', 27, 27, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (28, 1, 2, 4, 'R', 28, 28, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (29, 1, 2, 5, 'L', 29, 29, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (30, 1, 2, 5, 'R', 30, 30, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (31, 1, 2, 6, 'L', 31, 31, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (32, 1, 2, 6, 'R', 32, 32, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (33, 1, 2, 7, 'L', 33, 33, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (34, 1, 2, 7, 'R', 34, 34, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (35, 1, 2, 8, 'L', 35, 35, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (36, 1, 2, 8, 'R', 36, 36, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (37, 1, 2, 9, 'L', 37, 37, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (38, 1, 2, 9, 'R', 38, 38, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (39, 1, 2, 10, 'L', 39, 39, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (40, 1, 2, 10, 'R', 40, 40, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (41, 1, 3, 1, 'L', 41, 41, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (42, 1, 3, 1, 'R', 42, 42, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (43, 1, 3, 2, 'L', 43, 43, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (44, 1, 3, 2, 'R', 44, 44, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (45, 1, 3, 3, 'L', 45, 45, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (46, 1, 3, 3, 'R', 46, 46, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (47, 1, 3, 4, 'L', 47, 47, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (48, 1, 3, 4, 'R', 48, 48, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (49, 1, 3, 5, 'L', 49, 49, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (50, 1, 3, 5, 'R', 50, 50, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (51, 1, 3, 6, 'L', 51, 51, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (52, 1, 3, 6, 'R', 52, 52, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (53, 1, 3, 7, 'L', 53, 53, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (54, 1, 3, 7, 'R', 54, 54, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (55, 1, 3, 8, 'L', 55, 55, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (56, 1, 3, 8, 'R', 56, 56, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (57, 1, 3, 9, 'L', 57, 57, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (58, 1, 3, 9, 'R', 58, 58, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (59, 1, 3, 10, 'L', 59, 59, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (60, 1, 3, 10, 'R', 60, 60, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (61, 1, 4, 1, 'L', 61, 61, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (62, 1, 4, 1, 'R', 62, 62, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (63, 1, 4, 2, 'L', 63, 63, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (64, 1, 4, 2, 'R', 64, 64, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (65, 1, 4, 3, 'L', 65, 65, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (66, 1, 4, 3, 'R', 66, 66, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (67, 1, 4, 4, 'L', 67, 67, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (68, 1, 4, 4, 'R', 68, 68, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (69, 1, 4, 5, 'L', 69, 69, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (70, 1, 4, 5, 'R', 70, 70, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (71, 1, 4, 6, 'L', 71, 71, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (72, 1, 4, 6, 'R', 72, 72, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (73, 1, 4, 7, 'L', 73, 73, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (74, 1, 4, 7, 'R', 74, 74, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (75, 1, 4, 8, 'L', 75, 75, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (76, 1, 4, 8, 'R', 76, 76, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (77, 1, 4, 9, 'L', 77, 77, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (78, 1, 4, 9, 'R', 78, 78, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (79, 1, 4, 10, 'L', 79, 79, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (80, 1, 4, 10, 'R', 80, 80, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (81, 1, 5, 1, 'L', 81, 81, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (82, 1, 5, 1, 'R', 82, 82, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (83, 1, 5, 2, 'L', 83, 83, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (84, 1, 5, 2, 'R', 84, 84, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (85, 1, 5, 3, 'L', 85, 85, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (86, 1, 5, 3, 'R', 86, 86, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (87, 1, 5, 4, 'L', 87, 87, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (88, 1, 5, 4, 'R', 88, 88, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (89, 1, 5, 5, 'L', 89, 89, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (90, 1, 5, 5, 'R', 90, 90, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (91, 1, 5, 6, 'L', 91, 91, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (92, 1, 5, 6, 'R', 92, 92, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (93, 1, 5, 7, 'L', 93, 93, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (94, 1, 5, 7, 'R', 94, 94, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (95, 1, 5, 8, 'L', 95, 95, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (96, 1, 5, 8, 'R', 96, 96, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (97, 1, 5, 9, 'L', 97, 97, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (98, 1, 5, 9, 'R', 98, 98, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (99, 1, 5, 10, 'L', 99, 99, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (100, 1, 5, 10, 'R', 100, 100, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (101, 2, 1, 1, 'L', 1, 1, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (102, 2, 1, 1, 'R', 2, 2, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (103, 2, 1, 2, 'L', 3, 3, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (104, 2, 1, 2, 'R', 4, 4, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (105, 2, 1, 3, 'L', 5, 5, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (106, 2, 1, 3, 'R', 6, 6, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (107, 2, 1, 4, 'L', 7, 7, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (108, 2, 1, 4, 'R', 8, 8, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (109, 2, 1, 5, 'L', 9, 9, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (110, 2, 1, 5, 'R', 10, 10, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (111, 2, 1, 6, 'L', 11, 11, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (112, 2, 1, 6, 'R', 12, 12, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (113, 2, 1, 7, 'L', 13, 13, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (114, 2, 1, 7, 'R', 14, 14, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (115, 2, 1, 8, 'L', 15, 15, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (116, 2, 1, 8, 'R', 16, 16, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (117, 2, 1, 9, 'L', 17, 17, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (118, 2, 1, 9, 'R', 18, 18, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (119, 2, 1, 10, 'L', 19, 19, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (120, 2, 1, 10, 'R', 20, 20, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (121, 2, 2, 1, 'L', 21, 21, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (122, 2, 2, 1, 'R', 22, 22, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (123, 2, 2, 2, 'L', 23, 23, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (124, 2, 2, 2, 'R', 24, 24, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (125, 2, 2, 3, 'L', 25, 25, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (126, 2, 2, 3, 'R', 26, 26, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (127, 2, 2, 4, 'L', 27, 27, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (128, 2, 2, 4, 'R', 28, 28, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (129, 2, 2, 5, 'L', 29, 29, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (130, 2, 2, 5, 'R', 30, 30, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (131, 2, 2, 6, 'L', 31, 31, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (132, 2, 2, 6, 'R', 32, 32, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (133, 2, 2, 7, 'L', 33, 33, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (134, 2, 2, 7, 'R', 34, 34, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (135, 2, 2, 8, 'L', 35, 35, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (136, 2, 2, 8, 'R', 36, 36, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (137, 2, 2, 9, 'L', 37, 37, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (138, 2, 2, 9, 'R', 38, 38, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (139, 2, 2, 10, 'L', 39, 39, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (140, 2, 2, 10, 'R', 40, 40, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (141, 2, 3, 1, 'L', 41, 41, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (142, 2, 3, 1, 'R', 42, 42, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (143, 2, 3, 2, 'L', 43, 43, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (144, 2, 3, 2, 'R', 44, 44, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (145, 2, 3, 3, 'L', 45, 45, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (146, 2, 3, 3, 'R', 46, 46, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (147, 2, 3, 4, 'L', 47, 47, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (148, 2, 3, 4, 'R', 48, 48, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (149, 2, 3, 5, 'L', 49, 49, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (150, 2, 3, 5, 'R', 50, 50, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (151, 2, 3, 6, 'L', 51, 51, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (152, 2, 3, 6, 'R', 52, 52, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (153, 2, 3, 7, 'L', 53, 53, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (154, 2, 3, 7, 'R', 54, 54, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (155, 2, 3, 8, 'L', 55, 55, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (156, 2, 3, 8, 'R', 56, 56, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (157, 2, 3, 9, 'L', 57, 57, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (158, 2, 3, 9, 'R', 58, 58, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (159, 2, 3, 10, 'L', 59, 59, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (160, 2, 3, 10, 'R', 60, 60, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (161, 2, 4, 1, 'L', 61, 61, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (162, 2, 4, 1, 'R', 62, 62, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (163, 2, 4, 2, 'L', 63, 63, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (164, 2, 4, 2, 'R', 64, 64, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (165, 2, 4, 3, 'L', 65, 65, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (166, 2, 4, 3, 'R', 66, 66, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (167, 2, 4, 4, 'L', 67, 67, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (168, 2, 4, 4, 'R', 68, 68, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (169, 2, 4, 5, 'L', 69, 69, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (170, 2, 4, 5, 'R', 70, 70, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (171, 2, 4, 6, 'L', 71, 71, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (172, 2, 4, 6, 'R', 72, 72, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (173, 2, 4, 7, 'L', 73, 73, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (174, 2, 4, 7, 'R', 74, 74, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (175, 2, 4, 8, 'L', 75, 75, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (176, 2, 4, 8, 'R', 76, 76, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (177, 2, 4, 9, 'L', 77, 77, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (178, 2, 4, 9, 'R', 78, 78, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (179, 2, 4, 10, 'L', 79, 79, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (180, 2, 4, 10, 'R', 80, 80, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (181, 2, 5, 1, 'L', 81, 81, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (182, 2, 5, 1, 'R', 82, 82, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (183, 2, 5, 2, 'L', 83, 83, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (184, 2, 5, 2, 'R', 84, 84, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (185, 2, 5, 3, 'L', 85, 85, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (186, 2, 5, 3, 'R', 86, 86, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (187, 2, 5, 4, 'L', 87, 87, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (188, 2, 5, 4, 'R', 88, 88, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (189, 2, 5, 5, 'L', 89, 89, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (190, 2, 5, 5, 'R', 90, 90, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (191, 2, 5, 6, 'L', 91, 91, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (192, 2, 5, 6, 'R', 92, 92, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (193, 2, 5, 7, 'L', 93, 93, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (194, 2, 5, 7, 'R', 94, 94, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (195, 2, 5, 8, 'L', 95, 95, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (196, 2, 5, 8, 'R', 96, 96, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (197, 2, 5, 9, 'L', 97, 97, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (198, 2, 5, 9, 'R', 98, 98, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (199, 2, 5, 10, 'L', 99, 99, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (200, 2, 5, 10, 'R', 100, 100, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (201, 3, 1, 1, 'L', 1, 1, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (202, 3, 1, 1, 'R', 2, 2, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (203, 3, 1, 2, 'L', 3, 3, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (204, 3, 1, 2, 'R', 4, 4, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (205, 3, 1, 3, 'L', 5, 5, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (206, 3, 1, 3, 'R', 6, 6, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (207, 3, 1, 4, 'L', 7, 7, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (208, 3, 1, 4, 'R', 8, 8, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (209, 3, 1, 5, 'L', 9, 9, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (210, 3, 1, 5, 'R', 10, 10, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (211, 3, 1, 6, 'L', 11, 11, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (212, 3, 1, 6, 'R', 12, 12, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (213, 3, 1, 7, 'L', 13, 13, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (214, 3, 1, 7, 'R', 14, 14, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (215, 3, 1, 8, 'L', 15, 15, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (216, 3, 1, 8, 'R', 16, 16, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (217, 3, 1, 9, 'L', 17, 17, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (218, 3, 1, 9, 'R', 18, 18, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (219, 3, 1, 10, 'L', 19, 19, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (220, 3, 1, 10, 'R', 20, 20, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (221, 3, 2, 1, 'L', 21, 21, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (222, 3, 2, 1, 'R', 22, 22, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (223, 3, 2, 2, 'L', 23, 23, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (224, 3, 2, 2, 'R', 24, 24, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (225, 3, 2, 3, 'L', 25, 25, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (226, 3, 2, 3, 'R', 26, 26, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (227, 3, 2, 4, 'L', 27, 27, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (228, 3, 2, 4, 'R', 28, 28, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (229, 3, 2, 5, 'L', 29, 29, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (230, 3, 2, 5, 'R', 30, 30, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (231, 3, 2, 6, 'L', 31, 31, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (232, 3, 2, 6, 'R', 32, 32, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (233, 3, 2, 7, 'L', 33, 33, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (234, 3, 2, 7, 'R', 34, 34, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (235, 3, 2, 8, 'L', 35, 35, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (236, 3, 2, 8, 'R', 36, 36, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (237, 3, 2, 9, 'L', 37, 37, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (238, 3, 2, 9, 'R', 38, 38, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (239, 3, 2, 10, 'L', 39, 39, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (240, 3, 2, 10, 'R', 40, 40, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (241, 3, 3, 1, 'L', 41, 41, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (242, 3, 3, 1, 'R', 42, 42, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (243, 3, 3, 2, 'L', 43, 43, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (244, 3, 3, 2, 'R', 44, 44, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (245, 3, 3, 3, 'L', 45, 45, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (246, 3, 3, 3, 'R', 46, 46, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (247, 3, 3, 4, 'L', 47, 47, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (248, 3, 3, 4, 'R', 48, 48, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (249, 3, 3, 5, 'L', 49, 49, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (250, 3, 3, 5, 'R', 50, 50, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (251, 3, 3, 6, 'L', 51, 51, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (252, 3, 3, 6, 'R', 52, 52, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (253, 3, 3, 7, 'L', 53, 53, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (254, 3, 3, 7, 'R', 54, 54, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (255, 3, 3, 8, 'L', 55, 55, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (256, 3, 3, 8, 'R', 56, 56, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (257, 3, 3, 9, 'L', 57, 57, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (258, 3, 3, 9, 'R', 58, 58, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (259, 3, 3, 10, 'L', 59, 59, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (260, 3, 3, 10, 'R', 60, 60, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (261, 3, 4, 1, 'L', 61, 61, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (262, 3, 4, 1, 'R', 62, 62, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (263, 3, 4, 2, 'L', 63, 63, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (264, 3, 4, 2, 'R', 64, 64, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (265, 3, 4, 3, 'L', 65, 65, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (266, 3, 4, 3, 'R', 66, 66, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (267, 3, 4, 4, 'L', 67, 67, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (268, 3, 4, 4, 'R', 68, 68, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (269, 3, 4, 5, 'L', 69, 69, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (270, 3, 4, 5, 'R', 70, 70, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (271, 3, 4, 6, 'L', 71, 71, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (272, 3, 4, 6, 'R', 72, 72, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (273, 3, 4, 7, 'L', 73, 73, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (274, 3, 4, 7, 'R', 74, 74, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (275, 3, 4, 8, 'L', 75, 75, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (276, 3, 4, 8, 'R', 76, 76, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (277, 3, 4, 9, 'L', 77, 77, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (278, 3, 4, 9, 'R', 78, 78, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (279, 3, 4, 10, 'L', 79, 79, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (280, 3, 4, 10, 'R', 80, 80, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (281, 3, 5, 1, 'L', 81, 81, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (282, 3, 5, 1, 'R', 82, 82, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (283, 3, 5, 2, 'L', 83, 83, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (284, 3, 5, 2, 'R', 84, 84, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (285, 3, 5, 3, 'L', 85, 85, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (286, 3, 5, 3, 'R', 86, 86, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (287, 3, 5, 4, 'L', 87, 87, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (288, 3, 5, 4, 'R', 88, 88, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (289, 3, 5, 5, 'L', 89, 89, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (290, 3, 5, 5, 'R', 90, 90, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (291, 3, 5, 6, 'L', 91, 91, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (292, 3, 5, 6, 'R', 92, 92, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (293, 3, 5, 7, 'L', 93, 93, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (294, 3, 5, 7, 'R', 94, 94, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (295, 3, 5, 8, 'L', 95, 95, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (296, 3, 5, 8, 'R', 96, 96, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (297, 3, 5, 9, 'L', 97, 97, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (298, 3, 5, 9, 'R', 98, 98, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (299, 3, 5, 10, 'L', 99, 99, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (300, 3, 5, 10, 'R', 100, 100, 2, 'C', 1);
-- vertimag warehouse
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (301, 4, 1, 1, 'F', 1, 1, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (302, 4, 2, 1, 'F', 3, 3, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (303, 4, 3, 1, 'F', 5, 5, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (304, 4, 4, 1, 'F', 7, 7, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (305, 4, 5, 1, 'F', 9, 9, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (306, 4, 6, 1, 'F', 11, 11, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (307, 4, 7, 1, 'F', 13, 13, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (308, 4, 8, 1, 'F', 15, 15, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (309, 4, 9, 1, 'F', 17, 17, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (310, 4, 10, 1, 'F', 19, 19, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (311, 4, 1, 1, 'B', 2, 2, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (312, 4, 2, 1, 'B', 4, 4, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (313, 4, 3, 1, 'B', 6, 6, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (314, 4, 4, 1, 'B', 8, 8, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (315, 4, 5, 1, 'B', 10, 10, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (316, 4, 6, 1, 'B', 12, 12, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (317, 4, 7, 1, 'B', 14, 14, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (318, 4, 8, 1, 'B', 16, 16, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (319, 4, 9, 1, 'B', 18, 18, 3, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (320, 4, 10, 1, 'B', 20, 20, 3, 'A', 1);
SET IDENTITY_INSERT Cells OFF;

SET IDENTITY_INSERT CellConfigurations ON;
INSERT INTO CellConfigurations (Id, Description) VALUES (1, '1 tall pallet present');
INSERT INTO CellConfigurations (Id, Description) VALUES (2, '1 short pallet present');
INSERT INTO CellConfigurations (Id, Description) VALUES (3, 'Cell for Vertimag tray');
SET IDENTITY_INSERT CellConfigurations OFF;

INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (1, 1, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (2, 1, 2);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (2, 2, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 3, 1);

SET IDENTITY_INSERT CellPositions ON;
INSERT INTO CellPositions (Id, XOffset, YOffset, ZOffset, Description) VALUES (1, null, null, null, 'Pallet centered over cell');
INSERT INTO CellPositions (Id, XOffset, YOffset, ZOffset, Description) VALUES (2, null, null, null, 'Vertimag tray into cell');
SET IDENTITY_INSERT CellPositions OFF;


-- Loading Units
INSERT INTO LoadingUnitStatuses (Id, Description) VALUES ('U', 'Used');
INSERT INTO LoadingUnitStatuses (Id, Description) VALUES ('B', 'Blocked');
INSERT INTO LoadingUnitStatuses (Id, Description) VALUES ('A', 'Available');

SET IDENTITY_INSERT LoadingUnitWeightClasses ON;
INSERT INTO LoadingUnitWeightClasses (Id, Description, MinWeight, MaxWeight) VALUES (1, 'Pallet 1000kg weight max', 0, 1000);
INSERT INTO LoadingUnitWeightClasses (Id, Description, MinWeight, MaxWeight) VALUES (2, 'Pallet 500kg weight max - Vertimag', 0, 500);
SET IDENTITY_INSERT LoadingUnitWeightClasses OFF;

SET IDENTITY_INSERT LoadingUnitSizeClasses ON;
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (1, 'Europallet', 800, 1200, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (2, 'Vertimag tray 65XS', 1950, 650, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (3, 'Vertimag tray 84XS', 1950, 840, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (4, 'Vertimag tray 103XS', 1950, 1030, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (5, 'Vertimag tray 65S', 2450, 650, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (6, 'Vertimag tray 84S', 2450, 840, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (7, 'Vertimag tray 103S', 2450, 1030, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (8, 'Vertimag tray 65M', 3050, 650, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (9, 'Vertimag tray 84M', 3050, 840, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (10, 'Vertimag tray 103M', 3050, 1030, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (11, 'Vertimag tray 65L', 3650, 650, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (12, 'Vertimag tray 84L', 3650, 840, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (13, 'Vertimag tray 103L', 3650, 1030, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (14, 'Vertimag tray 65XL', 4250, 650, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (15, 'Vertimag tray 84XL', 4250, 840, 0, 0, 0, 0);
INSERT INTO LoadingUnitSizeClasses (Id, Description, Width, Length, BayOffset, Lift, BayForksUnthread, CellForksUnthread) VALUES (16, 'Vertimag tray 103XL', 4250, 1030, 0, 0, 0, 0);
SET IDENTITY_INSERT LoadingUnitSizeClasses OFF;

SET IDENTITY_INSERT LoadingUnitHeightClasses ON;
INSERT INTO LoadingUnitHeightClasses (Id, Description, MinHeight, MaxHeight) VALUES (1, 'Pallet 1300mm height max', 0, 1300);
INSERT INTO LoadingUnitHeightClasses (Id, Description, MinHeight, MaxHeight) VALUES (2, 'Pallet 1700mm height max', 1300, 1700);
INSERT INTO LoadingUnitHeightClasses (Id, Description, MinHeight, MaxHeight) VALUES (3, 'Cell 900mm height max - Vertimag', 0, 900);
SET IDENTITY_INSERT LoadingUnitHeightClasses OFF;

SET IDENTITY_INSERT LoadingUnitTypes ON;
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (1, 2, 1, 1, 'Europallet, 1700mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (2, 1, 1, 1, 'Europallet, 1300mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (3, 3, 2, 2, 'Vertimag tray 65XS, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (4, 3, 2, 3, 'Vertimag tray 84XS, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (5, 3, 2, 4, 'Vertimag tray 103XS, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (6, 3, 2, 5, 'Vertimag tray 65S, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (7, 3, 2, 6, 'Vertimag tray 84S, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (8, 3, 2, 7, 'Vertimag tray 103S, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (9, 3, 2, 8, 'Vertimag tray 65M, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (10, 3, 2, 9, 'Vertimag tray 84M, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (11, 3, 2, 10, 'Vertimag tray 103M, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (12, 3, 2, 11, 'Vertimag tray 65L, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (13, 3, 2, 12, 'Vertimag tray 84L, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (14, 3, 2, 13, 'Vertimag tray 103L, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (15, 3, 2, 14, 'Vertimag tray 65XL, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (16, 3, 2, 15, 'Vertimag tray 84XL, 900mm height max, 1000kg weight max');
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description) VALUES (17, 3, 2, 16, 'Vertimag tray 103XL, 900mm height max, 1000kg weight max');
SET IDENTITY_INSERT LoadingUnitTypes OFF;

INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (1, 1);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (2, 1);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (3, 1);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (1, 2);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (2, 2);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (3, 2);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (4, 3);

INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (1, 1, 1, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (1, 2, 2, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (2, 3, 3, 1);

SET IDENTITY_INSERT LoadingUnits ON;
INSERT INTO LoadingUnits (Id, Code, CellId, CellPairing, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (1, 'UDC1', 1, 'Free', 1, 1, 1600, 900, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 3, 6, 9);
INSERT INTO LoadingUnits (Id, Code, CellId, CellPairing, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (2, 'UDC2', 2, 'Free', 1, 1, 1600, 900, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 4, 8, 12);
INSERT INTO LoadingUnits (Id, Code, CellId, CellPairing, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (3, 'VERTIMAG1.1', 301, 'Free', 2, 15, 800, 400, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 5, 10, 15);
INSERT INTO LoadingUnits (Id, Code, CellId, CellPairing, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (4, 'VERTIMAG1.2', 302, 'Free', 2, 5, 700, 300, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 6, 12, 18);
SET IDENTITY_INSERT LoadingUnits OFF;

-- Compartments
SET IDENTITY_INSERT MaterialStatuses ON;
INSERT INTO MaterialStatuses (Id, Description) VALUES (1, 'Available');
INSERT INTO MaterialStatuses (Id, Description) VALUES (2, 'Awaiting verification');
INSERT INTO MaterialStatuses (Id, Description) VALUES (3, 'Expired');
INSERT INTO MaterialStatuses (Id, Description) VALUES (4, 'Blocked');
SET IDENTITY_INSERT MaterialStatuses OFF;

SET IDENTITY_INSERT PackageTypes ON;
INSERT INTO PackageTypes (Id, Description) VALUES (1, 'Free');
INSERT INTO PackageTypes (Id, Description) VALUES (2, 'Small packs');
INSERT INTO PackageTypes (Id, Description) VALUES (3, 'Medium packs');
INSERT INTO PackageTypes (Id, Description) VALUES (4, 'Boxes');
SET IDENTITY_INSERT PackageTypes OFF;

SET IDENTITY_INSERT CompartmentStatuses ON;
INSERT INTO CompartmentStatuses (Id, Description) VALUES (1, 'Free');
INSERT INTO CompartmentStatuses (Id, Description) VALUES (2, 'Used');
INSERT INTO CompartmentStatuses (Id, Description) VALUES (3, 'Empty');
INSERT INTO CompartmentStatuses (Id, Description) VALUES (4, 'Full');
INSERT INTO CompartmentStatuses (Id, Description) VALUES (5, 'Reserved');
INSERT INTO CompartmentStatuses (Id, Description) VALUES (6, 'Disabled');
SET IDENTITY_INSERT CompartmentStatuses OFF;

SET IDENTITY_INSERT CompartmentTypes ON;
INSERT INTO CompartmentTypes (Id, Description, Width, Height) VALUES (1, 'Full-pallet compartment type', 800, 1200);
INSERT INTO CompartmentTypes (Id, Description) VALUES (2, 'Vertimag general purpose compartment type');
INSERT INTO CompartmentTypes (Id, Description) VALUES (3, 'Vertimag other compartment type');
SET IDENTITY_INSERT CompartmentTypes OFF;

INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (1, 1, 100);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (2, 1, 100);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (2, 2, 200);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (2, 3, 300);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (2, 4, 400);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (2, 5, 500);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (2, 6, 600);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (3, 4, 1400);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (3, 5, 1500);
INSERT INTO ItemsCompartmentTypes (CompartmentTypeId, ItemId, MaxCapacity) VALUES (3, 6, 1600);

SET IDENTITY_INSERT Compartments ON;
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (1, 'UDC1_COMP01', 1, 1, 'Free', 1, 1, 1, 2, 5, 's1s1s1', 's2s2s2', 'llllll', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (2, 'UDC1_COMP02', 1, 1, 'Free', 1, 1, 1, 2, 10, 's3s3s3', 's4s4s4', 'mmmmmm', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (3, 'VERTIMAG1.1_COMP01', 3, 2, 'Free', 3, 1, 1, 2, 0, 's5s5s5', 's6s6s6', 'nnnnnn', 0, 0, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (4, 'VERTIMAG1.1_COMP02', 3, 2, 'Free', 3, 1, 1, 2, 5, 's7s7s7', 's8s8s8', 'pppppp', 0, 200, 200, 400, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (5, 'VERTIMAG1.1_COMP03', 3, 2, 'Free', 4, 1, 1, 2, 10, 's9s9s9', 's10s10s10', 'rrrrrr', 0, 600, 200, 50, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (6, 'VERTIMAG1.1_COMP04', 3, 2, 'Free', 4, 1, 1, 2, 20, 's11s11s11', 's12s12s12', 'tttttt', 200, 0, 400, 400, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (7, 'VERTIMAG1.1_COMP05', 3, 2, 'Free', 5, 1, 1, 2, 30, 's13s13s13', 's14s14s14', 'uuuuuu', 200, 400, 400, 250, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (8, 'VERTIMAG1.1_COMP06', 3, 2, 'Free', 5, 1, 1, 2, 40, 's15s15s15', 's16s16s16', 'vvvvvv', 600, 0, 200, 250, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (9, 'VERTIMAG1.1_COMP07', 3, 2, 'Free', 6, 1, 1, 2, 50, 's17s17s17', 's18s18s18', 'wwwwww', 600, 250, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (10, 'VERTIMAG1.1_COMP08', 3, 2, 'Free', 6, 1, 1, 2, 60, 's19s19s19', 's20s20s20', 'xxxxxx', 600, 450, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (11, 'VERTIMAG1.1_COMP09', 3, 2, 'Free', 6, 1, 1, 2, 70, 's21s21s21', 's22s22s22', 'yyyyyy', 800, 0, 200, 650, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (12, 'VERTIMAG1.1_COMP10', 3, 3, 'Free', 4, 1, 1, 2, 80, 's5s5s5', 's6s6s6', 'nnnnnn', 1000, 0, 500, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (13, 'VERTIMAG1.1_COMP11', 3, 3, 'Free', 5, 1, 1, 2, 90, 's7s7s7', 's8s8s8', 'pppppp', 1000, 200, 500, 400, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (14, 'VERTIMAG1.1_COMP12', 3, 3, 'Free', 6, 1, 1, 2, 100, 's9s9s9', 's10s10s10', 'rrrrrr', 1000, 600, 500, 50, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, Code, LoadingUnitId, CompartmentTypeId, ItemPairing, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (15, 'VERTIMAG1.2_COMP01', 4, 2, 'Free', 3, 1, 1, 2, 40, 'sss111', 'sss222', 'qqqqqq', 0, 0, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
SET IDENTITY_INSERT Compartments OFF;


-- Machines
INSERT INTO MachineTypes (Id, Description) VALUES ('T', 'Traslo');
INSERT INTO MachineTypes (Id, Description) VALUES ('S', 'Shuttle');
INSERT INTO MachineTypes (Id, Description) VALUES ('H', 'Handling');
INSERT INTO MachineTypes (Id, Description) VALUES ('L', 'LGV');
INSERT INTO MachineTypes (Id, Description) VALUES ('V', 'Vertimag');

SET IDENTITY_INSERT Machines ON;
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (1, 4, 'V', 'Vertimag 1', 'so74jnh0vyenf', 'MachineVertimagM', 'VMAG/ver-2019/variant-M', 125);
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (2, 5, 'V', 'Vertimag 2', 'msdy30yu76sb2', 'MachineVertimagXS', 'VMAG/ver-2018/variant-XS', 286);
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (3, 6, 'V', 'Vertimag 3', 'lwujg3ibg9h4j', 'MachineVertimagM', 'VMAG/ver-2018/variant-M', 78);
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (4, 7, 'V', 'Vertimag 4', '20fgn37o3nbe9', 'MachineVertimagXS', 'VMAG/ver-2019/variant-XS', 1904);
SET IDENTITY_INSERT Machines OFF;

-- Bay Types
DECLARE
    @BayTypes_Input char(1) = 'I',
    @BayTypes_Output char(1) = 'W',
    @BayTypes_Picking char(1) = 'P',
    @BayTypes_TrasloLoad char(1) = 'L',
    @BayTypes_TrasloUnload char(1) = 'U',
    @BayTypes_Vertimag char(1) = 'V';

-- Bays
INSERT INTO BayTypes (Id, Description) VALUES (@BayTypes_Input, 'Input Bay');
INSERT INTO BayTypes (Id, Description) VALUES (@BayTypes_Output, 'Output Bay');
INSERT INTO BayTypes (Id, Description) VALUES (@BayTypes_Picking, 'Picking Bay');
INSERT INTO BayTypes (Id, Description) VALUES (@BayTypes_TrasloLoad, 'Traslo load Bay');
INSERT INTO BayTypes (Id, Description) VALUES (@BayTypes_TrasloUnload, 'Traslo unload Bay');
INSERT INTO BayTypes (Id, Description) VALUES (@BayTypes_Vertimag, 'Vertimag Bay');

SET IDENTITY_INSERT Bays ON;
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (1, @BayTypes_Picking, 1, 'Single Pick Bay', @traslo_area, null);
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (2, @BayTypes_Vertimag, 1, 'Vertimag Bay', @vrtmag_area, 1);
SET IDENTITY_INSERT Bays OFF;

-- Operation Type
DECLARE
    @OperationType_Insertion char(1) = 'I',
    @OperationType_Withdrawal char(1) = 'W',
    @OperationType_Replacement char(1) = 'R',
    @OperationType_Reorder char(1) = 'O';

-- Scheduler Requests
SET IDENTITY_INSERT SchedulerRequests ON;
INSERT INTO SchedulerRequests (Id, IsInstant, ItemId, Sub1, RequestedQuantity, OperationType, AreaId) VALUES (1, 1, 1, 's1s1s1', 1, @OperationType_Withdrawal, @vrtmag_area);
INSERT INTO SchedulerRequests (Id, IsInstant, ItemId, Sub1, RequestedQuantity, OperationType, AreaId) VALUES (2, 1, 1, null, 1, @OperationType_Withdrawal, @vrtmag_area);
INSERT INTO SchedulerRequests (Id, IsInstant, ItemId, Sub1, RequestedQuantity, OperationType, AreaId) VALUES (3, 1, 1, null, 1, @OperationType_Withdrawal, @vrtmag_area);
SET IDENTITY_INSERT SchedulerRequests OFF;


--List
DECLARE
    @ItemList1_Id int = 1,
    @ItemList2_Id int = 2;

DECLARE
    @ItemListType_Put char(1) = 'U',
    @ItemListType_Pik char(1) = 'P',
    @ItemListType_Inv char(1) = 'I';

DECLARE
   @ItemListStatus_Exec char(1) = 'E',
   @ItemListStatus_Comp char(1) = 'C',
   @ItemListStatus_Incm char(1) = 'I',
   @ItemListStatus_Susp char(1) = 'S',
   @ItemListStatus_Wait char(1) = 'W';

SET IDENTITY_INSERT ItemLists ON;
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (@ItemList1_Id, 'Lista-1', @ItemListType_Pik, 'Prima Lista', 1, 'W', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (@ItemList2_Id, 'Lista-2', @ItemListType_Pik, 'Seconda Lista', 1, 'C', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (3,             'Lista-3', @ItemListType_Put, 'Terza Lista', 1, 'W', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (4,             'Lista-4', @ItemListType_Put, 'Quarta Lista', 1, 'C', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (5,             'Lista-5', @ItemListType_Pik, 'Quinta Lista', 1, 'I', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (6,             'Lista-6', @ItemListType_Put, 'Sesta Lista', 1, 'S', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (7,             'Lista-7', @ItemListType_Inv, 'Settima Lista', 1, 'E', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
SET IDENTITY_INSERT ItemLists OFF;

--List Row
SET IDENTITY_INSERT ItemListRows ON;
INSERT INTO ItemListRows (Id, ItemListId, Code, Priority, ItemId, MaterialStatusId, PackageTypeId, Lot, RequiredQuantity, EvadedQuantity, Status) VALUES (1, @ItemList1_Id, 'Code 1', 1, 1, 1, 1, 'Primo Item'  , 5, 10, @ItemListStatus_Wait);
INSERT INTO ItemListRows (Id, ItemListId, Code, Priority, ItemId, MaterialStatusId, PackageTypeId, Lot, RequiredQuantity, EvadedQuantity, Status) VALUES (2, @ItemList1_Id, 'Code 2', 1, 1, 1, 1, 'Secondo Item', 8, 20, @ItemListStatus_Wait);
INSERT INTO ItemListRows (Id, ItemListId, Code, Priority, ItemId, MaterialStatusId, PackageTypeId, Lot, RequiredQuantity, EvadedQuantity, Status) VALUES (3, @ItemList1_Id, 'Code 3', 1, 1, 1, 1, 'Terzo Item'  , 2, 10, @ItemListStatus_Comp);
SET IDENTITY_INSERT ItemListRows OFF;


COMMIT;
