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
    @manual_area int = 1,
    @vrtmag_area int = 2,

    @manual_aisle1 int = 1,
    @manual_aisle2 int = 2,
    @manual_aisle3 int = 3,

    @vrtmag_aisle1 int = 4,
    @vrtmag_aisle2 int = 5,
    @vrtmag_aisle3 int = 6,
    @vrtmag_aisle4 int = 7;

SET IDENTITY_INSERT Areas ON;
INSERT INTO Areas (Id, Name) VALUES (@manual_area, 'Manual Area');
INSERT INTO Areas (Id, Name) VALUES (@vrtmag_area, 'Vertimag Area');
SET IDENTITY_INSERT Areas OFF;

SET IDENTITY_INSERT Aisles ON;
INSERT INTO Aisles (Id, Name, AreaId, Floors, Columns) VALUES (1, 'Manual Aisle 1', @manual_area, 5, 10);
INSERT INTO Aisles (Id, Name, AreaId, Floors, Columns) VALUES (2, 'Manual Aisle 2', @manual_area, 5, 10);
INSERT INTO Aisles (Id, Name, AreaId, Floors, Columns) VALUES (3, 'Manual Aisle 3', @manual_area, 5, 10);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (@vrtmag_aisle1, 'Vertimag 1', @vrtmag_area);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (@vrtmag_aisle2, 'Vertimag 2', @vrtmag_area);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (@vrtmag_aisle3, 'Vertimag 3', @vrtmag_area);
INSERT INTO Aisles (Id, Name, AreaId)                  VALUES (@vrtmag_aisle4, 'Vertimag 4', @vrtmag_area);
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
INSERT INTO ItemCategories (Id, Description) VALUES (1, 'Screws');
INSERT INTO ItemCategories (Id, Description) VALUES (2, 'Bolts');
INSERT INTO ItemCategories (Id, Description) VALUES (3, 'Washers');
INSERT INTO ItemCategories (Id, Description) VALUES (4, 'Tools');
INSERT INTO ItemCategories (Id, Description) VALUES (5, 'Cables');
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

INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (1, @manual_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (2, @manual_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (3, @manual_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (4, @manual_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (5, @manual_area);
INSERT INTO ItemsAreas (ItemId, AreaId) VALUES (6, @manual_area);
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
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (1, @manual_aisle1, 1, 1, 'L', 1, 1, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (2, @manual_aisle1, 1, 1, 'R', 2, 2, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (3, @manual_aisle1, 1, 2, 'L', 3, 3, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (4, @manual_aisle1, 1, 2, 'R', 4, 4, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (5, @manual_aisle1, 1, 3, 'L', 5, 5, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (6, @manual_aisle1, 1, 3, 'R', 6, 6, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (7, @manual_aisle1, 1, 4, 'L', 7, 7, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (8, @manual_aisle1, 1, 4, 'R', 8, 8, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (9, @manual_aisle1, 1, 5, 'L', 9, 9, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (10, @manual_aisle1, 1, 5, 'R', 10, 10, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (11, @manual_aisle1, 1, 6, 'L', 11, 11, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (12, @manual_aisle1, 1, 6, 'R', 12, 12, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (13, @manual_aisle1, 1, 7, 'L', 13, 13, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (14, @manual_aisle1, 1, 7, 'R', 14, 14, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (15, @manual_aisle1, 1, 8, 'L', 15, 15, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (16, @manual_aisle1, 1, 8, 'R', 16, 16, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (17, @manual_aisle1, 1, 9, 'L', 17, 17, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (18, @manual_aisle1, 1, 9, 'R', 18, 18, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (19, @manual_aisle1, 1, 10, 'L', 19, 19, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (20, @manual_aisle1, 1, 10, 'R', 20, 20, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (21, @manual_aisle1, 2, 1, 'L', 21, 21, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (22, @manual_aisle1, 2, 1, 'R', 22, 22, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (23, @manual_aisle1, 2, 2, 'L', 23, 23, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (24, @manual_aisle1, 2, 2, 'R', 24, 24, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (25, @manual_aisle1, 2, 3, 'L', 25, 25, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (26, @manual_aisle1, 2, 3, 'R', 26, 26, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (27, @manual_aisle1, 2, 4, 'L', 27, 27, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (28, @manual_aisle1, 2, 4, 'R', 28, 28, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (29, @manual_aisle1, 2, 5, 'L', 29, 29, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (30, @manual_aisle1, 2, 5, 'R', 30, 30, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (31, @manual_aisle1, 2, 6, 'L', 31, 31, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (32, @manual_aisle1, 2, 6, 'R', 32, 32, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (33, @manual_aisle1, 2, 7, 'L', 33, 33, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (34, @manual_aisle1, 2, 7, 'R', 34, 34, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (35, @manual_aisle1, 2, 8, 'L', 35, 35, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (36, @manual_aisle1, 2, 8, 'R', 36, 36, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (37, @manual_aisle1, 2, 9, 'L', 37, 37, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (38, @manual_aisle1, 2, 9, 'R', 38, 38, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (39, @manual_aisle1, 2, 10, 'L', 39, 39, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (40, @manual_aisle1, 2, 10, 'R', 40, 40, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (41, @manual_aisle1, 3, 1, 'L', 41, 41, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (42, @manual_aisle1, 3, 1, 'R', 42, 42, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (43, @manual_aisle1, 3, 2, 'L', 43, 43, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (44, @manual_aisle1, 3, 2, 'R', 44, 44, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (45, @manual_aisle1, 3, 3, 'L', 45, 45, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (46, @manual_aisle1, 3, 3, 'R', 46, 46, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (47, @manual_aisle1, 3, 4, 'L', 47, 47, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (48, @manual_aisle1, 3, 4, 'R', 48, 48, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (49, @manual_aisle1, 3, 5, 'L', 49, 49, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (50, @manual_aisle1, 3, 5, 'R', 50, 50, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (51, @manual_aisle1, 3, 6, 'L', 51, 51, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (52, @manual_aisle1, 3, 6, 'R', 52, 52, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (53, @manual_aisle1, 3, 7, 'L', 53, 53, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (54, @manual_aisle1, 3, 7, 'R', 54, 54, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (55, @manual_aisle1, 3, 8, 'L', 55, 55, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (56, @manual_aisle1, 3, 8, 'R', 56, 56, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (57, @manual_aisle1, 3, 9, 'L', 57, 57, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (58, @manual_aisle1, 3, 9, 'R', 58, 58, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (59, @manual_aisle1, 3, 10, 'L', 59, 59, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (60, @manual_aisle1, 3, 10, 'R', 60, 60, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (61, @manual_aisle1, 4, 1, 'L', 61, 61, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (62, @manual_aisle1, 4, 1, 'R', 62, 62, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (63, @manual_aisle1, 4, 2, 'L', 63, 63, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (64, @manual_aisle1, 4, 2, 'R', 64, 64, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (65, @manual_aisle1, 4, 3, 'L', 65, 65, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (66, @manual_aisle1, 4, 3, 'R', 66, 66, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (67, @manual_aisle1, 4, 4, 'L', 67, 67, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (68, @manual_aisle1, 4, 4, 'R', 68, 68, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (69, @manual_aisle1, 4, 5, 'L', 69, 69, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (70, @manual_aisle1, 4, 5, 'R', 70, 70, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (71, @manual_aisle1, 4, 6, 'L', 71, 71, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (72, @manual_aisle1, 4, 6, 'R', 72, 72, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (73, @manual_aisle1, 4, 7, 'L', 73, 73, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (74, @manual_aisle1, 4, 7, 'R', 74, 74, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (75, @manual_aisle1, 4, 8, 'L', 75, 75, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (76, @manual_aisle1, 4, 8, 'R', 76, 76, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (77, @manual_aisle1, 4, 9, 'L', 77, 77, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (78, @manual_aisle1, 4, 9, 'R', 78, 78, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (79, @manual_aisle1, 4, 10, 'L', 79, 79, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (80, @manual_aisle1, 4, 10, 'R', 80, 80, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (81, @manual_aisle1, 5, 1, 'L', 81, 81, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (82, @manual_aisle1, 5, 1, 'R', 82, 82, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (83, @manual_aisle1, 5, 2, 'L', 83, 83, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (84, @manual_aisle1, 5, 2, 'R', 84, 84, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (85, @manual_aisle1, 5, 3, 'L', 85, 85, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (86, @manual_aisle1, 5, 3, 'R', 86, 86, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (87, @manual_aisle1, 5, 4, 'L', 87, 87, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (88, @manual_aisle1, 5, 4, 'R', 88, 88, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (89, @manual_aisle1, 5, 5, 'L', 89, 89, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (90, @manual_aisle1, 5, 5, 'R', 90, 90, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (91, @manual_aisle1, 5, 6, 'L', 91, 91, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (92, @manual_aisle1, 5, 6, 'R', 92, 92, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (93, @manual_aisle1, 5, 7, 'L', 93, 93, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (94, @manual_aisle1, 5, 7, 'R', 94, 94, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (95, @manual_aisle1, 5, 8, 'L', 95, 95, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (96, @manual_aisle1, 5, 8, 'R', 96, 96, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (97, @manual_aisle1, 5, 9, 'L', 97, 97, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (98, @manual_aisle1, 5, 9, 'R', 98, 98, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (99, @manual_aisle1, 5, 10, 'L', 99, 99, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (100, @manual_aisle1, 5, 10, 'R', 100, 100, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (101, @manual_aisle2, 1, 1, 'L', 1, 1, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (102, @manual_aisle2, 1, 1, 'R', 2, 2, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (103, @manual_aisle2, 1, 2, 'L', 3, 3, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (104, @manual_aisle2, 1, 2, 'R', 4, 4, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (105, @manual_aisle2, 1, 3, 'L', 5, 5, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (106, @manual_aisle2, 1, 3, 'R', 6, 6, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (107, @manual_aisle2, 1, 4, 'L', 7, 7, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (108, @manual_aisle2, 1, 4, 'R', 8, 8, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (109, @manual_aisle2, 1, 5, 'L', 9, 9, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (110, @manual_aisle2, 1, 5, 'R', 10, 10, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (111, @manual_aisle2, 1, 6, 'L', 11, 11, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (112, @manual_aisle2, 1, 6, 'R', 12, 12, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (113, @manual_aisle2, 1, 7, 'L', 13, 13, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (114, @manual_aisle2, 1, 7, 'R', 14, 14, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (115, @manual_aisle2, 1, 8, 'L', 15, 15, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (116, @manual_aisle2, 1, 8, 'R', 16, 16, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (117, @manual_aisle2, 1, 9, 'L', 17, 17, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (118, @manual_aisle2, 1, 9, 'R', 18, 18, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (119, @manual_aisle2, 1, 10, 'L', 19, 19, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (120, @manual_aisle2, 1, 10, 'R', 20, 20, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (121, @manual_aisle2, 2, 1, 'L', 21, 21, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (122, @manual_aisle2, 2, 1, 'R', 22, 22, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (123, @manual_aisle2, 2, 2, 'L', 23, 23, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (124, @manual_aisle2, 2, 2, 'R', 24, 24, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (125, @manual_aisle2, 2, 3, 'L', 25, 25, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (126, @manual_aisle2, 2, 3, 'R', 26, 26, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (127, @manual_aisle2, 2, 4, 'L', 27, 27, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (128, @manual_aisle2, 2, 4, 'R', 28, 28, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (129, @manual_aisle2, 2, 5, 'L', 29, 29, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (130, @manual_aisle2, 2, 5, 'R', 30, 30, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (131, @manual_aisle2, 2, 6, 'L', 31, 31, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (132, @manual_aisle2, 2, 6, 'R', 32, 32, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (133, @manual_aisle2, 2, 7, 'L', 33, 33, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (134, @manual_aisle2, 2, 7, 'R', 34, 34, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (135, @manual_aisle2, 2, 8, 'L', 35, 35, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (136, @manual_aisle2, 2, 8, 'R', 36, 36, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (137, @manual_aisle2, 2, 9, 'L', 37, 37, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (138, @manual_aisle2, 2, 9, 'R', 38, 38, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (139, @manual_aisle2, 2, 10, 'L', 39, 39, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (140, @manual_aisle2, 2, 10, 'R', 40, 40, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (141, @manual_aisle2, 3, 1, 'L', 41, 41, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (142, @manual_aisle2, 3, 1, 'R', 42, 42, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (143, @manual_aisle2, 3, 2, 'L', 43, 43, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (144, @manual_aisle2, 3, 2, 'R', 44, 44, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (145, @manual_aisle2, 3, 3, 'L', 45, 45, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (146, @manual_aisle2, 3, 3, 'R', 46, 46, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (147, @manual_aisle2, 3, 4, 'L', 47, 47, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (148, @manual_aisle2, 3, 4, 'R', 48, 48, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (149, @manual_aisle2, 3, 5, 'L', 49, 49, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (150, @manual_aisle2, 3, 5, 'R', 50, 50, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (151, @manual_aisle2, 3, 6, 'L', 51, 51, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (152, @manual_aisle2, 3, 6, 'R', 52, 52, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (153, @manual_aisle2, 3, 7, 'L', 53, 53, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (154, @manual_aisle2, 3, 7, 'R', 54, 54, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (155, @manual_aisle2, 3, 8, 'L', 55, 55, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (156, @manual_aisle2, 3, 8, 'R', 56, 56, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (157, @manual_aisle2, 3, 9, 'L', 57, 57, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (158, @manual_aisle2, 3, 9, 'R', 58, 58, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (159, @manual_aisle2, 3, 10, 'L', 59, 59, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (160, @manual_aisle2, 3, 10, 'R', 60, 60, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (161, @manual_aisle2, 4, 1, 'L', 61, 61, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (162, @manual_aisle2, 4, 1, 'R', 62, 62, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (163, @manual_aisle2, 4, 2, 'L', 63, 63, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (164, @manual_aisle2, 4, 2, 'R', 64, 64, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (165, @manual_aisle2, 4, 3, 'L', 65, 65, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (166, @manual_aisle2, 4, 3, 'R', 66, 66, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (167, @manual_aisle2, 4, 4, 'L', 67, 67, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (168, @manual_aisle2, 4, 4, 'R', 68, 68, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (169, @manual_aisle2, 4, 5, 'L', 69, 69, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (170, @manual_aisle2, 4, 5, 'R', 70, 70, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (171, @manual_aisle2, 4, 6, 'L', 71, 71, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (172, @manual_aisle2, 4, 6, 'R', 72, 72, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (173, @manual_aisle2, 4, 7, 'L', 73, 73, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (174, @manual_aisle2, 4, 7, 'R', 74, 74, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (175, @manual_aisle2, 4, 8, 'L', 75, 75, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (176, @manual_aisle2, 4, 8, 'R', 76, 76, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (177, @manual_aisle2, 4, 9, 'L', 77, 77, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (178, @manual_aisle2, 4, 9, 'R', 78, 78, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (179, @manual_aisle2, 4, 10, 'L', 79, 79, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (180, @manual_aisle2, 4, 10, 'R', 80, 80, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (181, @manual_aisle2, 5, 1, 'L', 81, 81, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (182, @manual_aisle2, 5, 1, 'R', 82, 82, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (183, @manual_aisle2, 5, 2, 'L', 83, 83, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (184, @manual_aisle2, 5, 2, 'R', 84, 84, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (185, @manual_aisle2, 5, 3, 'L', 85, 85, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (186, @manual_aisle2, 5, 3, 'R', 86, 86, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (187, @manual_aisle2, 5, 4, 'L', 87, 87, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (188, @manual_aisle2, 5, 4, 'R', 88, 88, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (189, @manual_aisle2, 5, 5, 'L', 89, 89, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (190, @manual_aisle2, 5, 5, 'R', 90, 90, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (191, @manual_aisle2, 5, 6, 'L', 91, 91, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (192, @manual_aisle2, 5, 6, 'R', 92, 92, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (193, @manual_aisle2, 5, 7, 'L', 93, 93, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (194, @manual_aisle2, 5, 7, 'R', 94, 94, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (195, @manual_aisle2, 5, 8, 'L', 95, 95, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (196, @manual_aisle2, 5, 8, 'R', 96, 96, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (197, @manual_aisle2, 5, 9, 'L', 97, 97, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (198, @manual_aisle2, 5, 9, 'R', 98, 98, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (199, @manual_aisle2, 5, 10, 'L', 99, 99, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (200, @manual_aisle2, 5, 10, 'R', 100, 100, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (201, @manual_aisle3, 1, 1, 'L', 1, 1, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (202, @manual_aisle3, 1, 1, 'R', 2, 2, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (203, @manual_aisle3, 1, 2, 'L', 3, 3, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (204, @manual_aisle3, 1, 2, 'R', 4, 4, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (205, @manual_aisle3, 1, 3, 'L', 5, 5, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (206, @manual_aisle3, 1, 3, 'R', 6, 6, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (207, @manual_aisle3, 1, 4, 'L', 7, 7, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (208, @manual_aisle3, 1, 4, 'R', 8, 8, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (209, @manual_aisle3, 1, 5, 'L', 9, 9, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (210, @manual_aisle3, 1, 5, 'R', 10, 10, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (211, @manual_aisle3, 1, 6, 'L', 11, 11, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (212, @manual_aisle3, 1, 6, 'R', 12, 12, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (213, @manual_aisle3, 1, 7, 'L', 13, 13, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (214, @manual_aisle3, 1, 7, 'R', 14, 14, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (215, @manual_aisle3, 1, 8, 'L', 15, 15, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (216, @manual_aisle3, 1, 8, 'R', 16, 16, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (217, @manual_aisle3, 1, 9, 'L', 17, 17, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (218, @manual_aisle3, 1, 9, 'R', 18, 18, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (219, @manual_aisle3, 1, 10, 'L', 19, 19, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (220, @manual_aisle3, 1, 10, 'R', 20, 20, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (221, @manual_aisle3, 2, 1, 'L', 21, 21, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (222, @manual_aisle3, 2, 1, 'R', 22, 22, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (223, @manual_aisle3, 2, 2, 'L', 23, 23, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (224, @manual_aisle3, 2, 2, 'R', 24, 24, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (225, @manual_aisle3, 2, 3, 'L', 25, 25, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (226, @manual_aisle3, 2, 3, 'R', 26, 26, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (227, @manual_aisle3, 2, 4, 'L', 27, 27, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (228, @manual_aisle3, 2, 4, 'R', 28, 28, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (229, @manual_aisle3, 2, 5, 'L', 29, 29, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (230, @manual_aisle3, 2, 5, 'R', 30, 30, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (231, @manual_aisle3, 2, 6, 'L', 31, 31, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (232, @manual_aisle3, 2, 6, 'R', 32, 32, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (233, @manual_aisle3, 2, 7, 'L', 33, 33, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (234, @manual_aisle3, 2, 7, 'R', 34, 34, 1, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (235, @manual_aisle3, 2, 8, 'L', 35, 35, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (236, @manual_aisle3, 2, 8, 'R', 36, 36, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (237, @manual_aisle3, 2, 9, 'L', 37, 37, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (238, @manual_aisle3, 2, 9, 'R', 38, 38, 1, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (239, @manual_aisle3, 2, 10, 'L', 39, 39, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (240, @manual_aisle3, 2, 10, 'R', 40, 40, 1, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (241, @manual_aisle3, 3, 1, 'L', 41, 41, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (242, @manual_aisle3, 3, 1, 'R', 42, 42, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (243, @manual_aisle3, 3, 2, 'L', 43, 43, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (244, @manual_aisle3, 3, 2, 'R', 44, 44, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (245, @manual_aisle3, 3, 3, 'L', 45, 45, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (246, @manual_aisle3, 3, 3, 'R', 46, 46, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (247, @manual_aisle3, 3, 4, 'L', 47, 47, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (248, @manual_aisle3, 3, 4, 'R', 48, 48, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (249, @manual_aisle3, 3, 5, 'L', 49, 49, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (250, @manual_aisle3, 3, 5, 'R', 50, 50, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (251, @manual_aisle3, 3, 6, 'L', 51, 51, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (252, @manual_aisle3, 3, 6, 'R', 52, 52, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (253, @manual_aisle3, 3, 7, 'L', 53, 53, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (254, @manual_aisle3, 3, 7, 'R', 54, 54, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (255, @manual_aisle3, 3, 8, 'L', 55, 55, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (256, @manual_aisle3, 3, 8, 'R', 56, 56, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (257, @manual_aisle3, 3, 9, 'L', 57, 57, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (258, @manual_aisle3, 3, 9, 'R', 58, 58, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (259, @manual_aisle3, 3, 10, 'L', 59, 59, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (260, @manual_aisle3, 3, 10, 'R', 60, 60, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (261, @manual_aisle3, 4, 1, 'L', 61, 61, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (262, @manual_aisle3, 4, 1, 'R', 62, 62, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (263, @manual_aisle3, 4, 2, 'L', 63, 63, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (264, @manual_aisle3, 4, 2, 'R', 64, 64, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (265, @manual_aisle3, 4, 3, 'L', 65, 65, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (266, @manual_aisle3, 4, 3, 'R', 66, 66, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (267, @manual_aisle3, 4, 4, 'L', 67, 67, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (268, @manual_aisle3, 4, 4, 'R', 68, 68, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (269, @manual_aisle3, 4, 5, 'L', 69, 69, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (270, @manual_aisle3, 4, 5, 'R', 70, 70, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (271, @manual_aisle3, 4, 6, 'L', 71, 71, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (272, @manual_aisle3, 4, 6, 'R', 72, 72, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (273, @manual_aisle3, 4, 7, 'L', 73, 73, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (274, @manual_aisle3, 4, 7, 'R', 74, 74, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (275, @manual_aisle3, 4, 8, 'L', 75, 75, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (276, @manual_aisle3, 4, 8, 'R', 76, 76, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (277, @manual_aisle3, 4, 9, 'L', 77, 77, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (278, @manual_aisle3, 4, 9, 'R', 78, 78, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (279, @manual_aisle3, 4, 10, 'L', 79, 79, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (280, @manual_aisle3, 4, 10, 'R', 80, 80, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (281, @manual_aisle3, 5, 1, 'L', 81, 81, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (282, @manual_aisle3, 5, 1, 'R', 82, 82, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (283, @manual_aisle3, 5, 2, 'L', 83, 83, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (284, @manual_aisle3, 5, 2, 'R', 84, 84, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (285, @manual_aisle3, 5, 3, 'L', 85, 85, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (286, @manual_aisle3, 5, 3, 'R', 86, 86, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (287, @manual_aisle3, 5, 4, 'L', 87, 87, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (288, @manual_aisle3, 5, 4, 'R', 88, 88, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (289, @manual_aisle3, 5, 5, 'L', 89, 89, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (290, @manual_aisle3, 5, 5, 'R', 90, 90, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (291, @manual_aisle3, 5, 6, 'L', 91, 91, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (292, @manual_aisle3, 5, 6, 'R', 92, 92, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (293, @manual_aisle3, 5, 7, 'L', 93, 93, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (294, @manual_aisle3, 5, 7, 'R', 94, 94, 2, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (295, @manual_aisle3, 5, 8, 'L', 95, 95, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (296, @manual_aisle3, 5, 8, 'R', 96, 96, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (297, @manual_aisle3, 5, 9, 'L', 97, 97, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (298, @manual_aisle3, 5, 9, 'R', 98, 98, 2, 'B', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (299, @manual_aisle3, 5, 10, 'L', 99, 99, 2, 'C', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (300, @manual_aisle3, 5, 10, 'R', 100, 100, 2, 'C', 1);
-- vertimag warehouses
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (301, @vrtmag_aisle1, 1, 1, 'F', 1, 1, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (302, @vrtmag_aisle1, 2, 1, 'F', 3, 3, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (303, @vrtmag_aisle1, 3, 1, 'F', 5, 5, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (304, @vrtmag_aisle1, 4, 1, 'F', 7, 7, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (305, @vrtmag_aisle1, 5, 1, 'F', 9, 9, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (306, @vrtmag_aisle1, 6, 1, 'F', 11, 11, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (307, @vrtmag_aisle1, 7, 1, 'F', 13, 13, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (308, @vrtmag_aisle1, 8, 1, 'F', 15, 15, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (309, @vrtmag_aisle1, 9, 1, 'F', 17, 17, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (310, @vrtmag_aisle1, 10, 1, 'F', 19, 19, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (311, @vrtmag_aisle1, 1, 1, 'B', 2, 2, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (312, @vrtmag_aisle1, 2, 1, 'B', 4, 4, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (313, @vrtmag_aisle1, 3, 1, 'B', 6, 6, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (314, @vrtmag_aisle1, 4, 1, 'B', 8, 8, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (315, @vrtmag_aisle1, 5, 1, 'B', 10, 10, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (316, @vrtmag_aisle1, 6, 1, 'B', 12, 12, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (317, @vrtmag_aisle1, 7, 1, 'B', 14, 14, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (318, @vrtmag_aisle1, 8, 1, 'B', 16, 16, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (319, @vrtmag_aisle1, 9, 1, 'B', 18, 18, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (320, @vrtmag_aisle1, 10, 1, 'B', 20, 20, 15, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (321, @vrtmag_aisle2, 1, 1, 'F', 1, 1, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (322, @vrtmag_aisle2, 2, 1, 'F', 3, 3, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (323, @vrtmag_aisle2, 3, 1, 'F', 5, 5, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (324, @vrtmag_aisle2, 4, 1, 'F', 7, 7, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (325, @vrtmag_aisle2, 5, 1, 'F', 9, 9, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (326, @vrtmag_aisle2, 6, 1, 'F', 11, 11, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (327, @vrtmag_aisle2, 7, 1, 'F', 13, 13, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (328, @vrtmag_aisle2, 8, 1, 'F', 15, 15, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (329, @vrtmag_aisle2, 9, 1, 'F', 17, 17, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (330, @vrtmag_aisle2, 10, 1, 'F', 19, 19, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (331, @vrtmag_aisle2, 1, 1, 'B', 2, 2, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (332, @vrtmag_aisle2, 2, 1, 'B', 4, 4, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (333, @vrtmag_aisle2, 3, 1, 'B', 6, 6, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (334, @vrtmag_aisle2, 4, 1, 'B', 8, 8, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (335, @vrtmag_aisle2, 5, 1, 'B', 10, 10, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (336, @vrtmag_aisle2, 6, 1, 'B', 12, 12, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (337, @vrtmag_aisle2, 7, 1, 'B', 14, 14, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (338, @vrtmag_aisle2, 8, 1, 'B', 16, 16, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (339, @vrtmag_aisle2, 9, 1, 'B', 18, 18, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (340, @vrtmag_aisle2, 10, 1, 'B', 20, 20, 5, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (341, @vrtmag_aisle3, 1, 1, 'F', 1, 1, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (342, @vrtmag_aisle3, 2, 1, 'F', 3, 3, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (343, @vrtmag_aisle3, 3, 1, 'F', 5, 5, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (344, @vrtmag_aisle3, 4, 1, 'F', 7, 7, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (345, @vrtmag_aisle3, 5, 1, 'F', 9, 9, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (346, @vrtmag_aisle3, 6, 1, 'F', 11, 11, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (347, @vrtmag_aisle3, 7, 1, 'F', 13, 13, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (348, @vrtmag_aisle3, 8, 1, 'F', 15, 15, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (349, @vrtmag_aisle3, 9, 1, 'F', 17, 17, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (350, @vrtmag_aisle3, 10, 1, 'F', 19, 19, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (351, @vrtmag_aisle3, 1, 1, 'B', 2, 2, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (352, @vrtmag_aisle3, 2, 1, 'B', 4, 4, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (353, @vrtmag_aisle3, 3, 1, 'B', 6, 6, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (354, @vrtmag_aisle3, 4, 1, 'B', 8, 8, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (355, @vrtmag_aisle3, 5, 1, 'B', 10, 10, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (356, @vrtmag_aisle3, 6, 1, 'B', 12, 12, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (357, @vrtmag_aisle3, 7, 1, 'B', 14, 14, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (358, @vrtmag_aisle3, 8, 1, 'B', 16, 16, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (359, @vrtmag_aisle3, 9, 1, 'B', 18, 18, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (360, @vrtmag_aisle3, 10, 1, 'B', 20, 20, 10, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (361, @vrtmag_aisle4, 1, 1, 'F', 1, 1, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (362, @vrtmag_aisle4, 2, 1, 'F', 3, 3, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (363, @vrtmag_aisle4, 3, 1, 'F', 5, 5, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (364, @vrtmag_aisle4, 4, 1, 'F', 7, 7, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (365, @vrtmag_aisle4, 5, 1, 'F', 9, 9, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (366, @vrtmag_aisle4, 6, 1, 'F', 11, 11, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (367, @vrtmag_aisle4, 7, 1, 'F', 13, 13, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (368, @vrtmag_aisle4, 8, 1, 'F', 15, 15, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (369, @vrtmag_aisle4, 9, 1, 'F', 17, 17, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (370, @vrtmag_aisle4, 10, 1, 'F', 19, 19, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (371, @vrtmag_aisle4, 1, 1, 'B', 2, 2, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (372, @vrtmag_aisle4, 2, 1, 'B', 4, 4, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (373, @vrtmag_aisle4, 3, 1, 'B', 6, 6, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (374, @vrtmag_aisle4, 4, 1, 'B', 8, 8, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (375, @vrtmag_aisle4, 5, 1, 'B', 10, 10, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (376, @vrtmag_aisle4, 6, 1, 'B', 12, 12, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (377, @vrtmag_aisle4, 7, 1, 'B', 14, 14, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (378, @vrtmag_aisle4, 8, 1, 'B', 16, 16, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (379, @vrtmag_aisle4, 9, 1, 'B', 18, 18, 13, 'A', 1);
INSERT INTO Cells (Id, AisleId, Floor, [Column], Side, CellNumber, Priority, CellTypeId, AbcClassId, CellStatusId) VALUES (380, @vrtmag_aisle4, 10, 1, 'B', 20, 20, 13, 'A', 1);
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
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 4, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 5, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 6, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 7, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 8, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 9, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 10, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 11, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 12, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 13, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 14, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 15, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 16, 1);
INSERT INTO CellConfigurationCellTypes (CellConfigurationId, CellTypeId, Priority) VALUES (3, 17, 1);

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
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (1, 2, 1, 1, 'Europallet, 1700mm height max, 1000kg weight max', 0);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (2, 1, 1, 1, 'Europallet, 1300mm height max, 1000kg weight max', 0);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (3, 3, 2, 2, 'Vertimag tray 65XS, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (4, 3, 2, 3, 'Vertimag tray 84XS, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (5, 3, 2, 4, 'Vertimag tray 103XS, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (6, 3, 2, 5, 'Vertimag tray 65S, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (7, 3, 2, 6, 'Vertimag tray 84S, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (8, 3, 2, 7, 'Vertimag tray 103S, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (9, 3, 2, 8, 'Vertimag tray 65M, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (10, 3, 2, 9, 'Vertimag tray 84M, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (11, 3, 2, 10, 'Vertimag tray 103M, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (12, 3, 2, 11, 'Vertimag tray 65L, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (13, 3, 2, 12, 'Vertimag tray 84L, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (14, 3, 2, 13, 'Vertimag tray 103L, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (15, 3, 2, 14, 'Vertimag tray 65XL, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (16, 3, 2, 15, 'Vertimag tray 84XL, 900mm height max, 1000kg weight max', 1);
INSERT INTO LoadingUnitTypes (Id, LoadingUnitHeightClassId, LoadingUnitWeightClassId, LoadingUnitSizeClassId, Description, HasCompartments) VALUES (17, 3, 2, 16, 'Vertimag tray 103XL, 900mm height max, 1000kg weight max', 1);
SET IDENTITY_INSERT LoadingUnitTypes OFF;

INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@manual_aisle1, 1);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@manual_aisle2, 1);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@manual_aisle3, 1);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@manual_aisle1, 2);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@manual_aisle2, 2);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@manual_aisle3, 2);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@vrtmag_aisle1, 15);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@vrtmag_aisle2, 5);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@vrtmag_aisle3, 10);
INSERT INTO LoadingUnitTypesAisles (AisleId, LoadingUnitTypeId) VALUES (@vrtmag_aisle4, 13);

INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (1, 1, 1, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (1, 2, 2, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (2, 3, 15, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (2, 3, 5, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (2, 3, 10, 1);
INSERT INTO CellConfigurationCellPositionLoadingUnitTypes (CellPositionId, CellConfigurationId, LoadingUnitTypeId, Priority) VALUES (2, 3, 13, 1);

SET IDENTITY_INSERT LoadingUnits ON;
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (1, 'UDC1', 1, 0, 1, 1, 1600, 900, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 3, 6, 9);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (2, 'UDC2', 2, 0, 1, 1, 1600, 900, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 4, 8, 12);

INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (3, 'VERTIMAG1.1', 301, 0, 2, 15, 800, 400, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 5, 10, 15);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (4, 'VERTIMAG1.2', 302, 0, 2, 15, 700, 350, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 6, 12, 18);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (5, 'VERTIMAG1.3', 303, 0, 2, 15, 600, 300, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 7, 14, 21);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (6, 'VERTIMAG1.4', 304, 0, 2, 15, 500, 250, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 8, 16, 24);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (7, 'VERTIMAG1.5', 305, 0, 2, 15, 400, 200, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 9, 18, 27);

INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (8, 'VERTIMAG2.1', 321, 0, 2, 5, 800, 400, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 5, 10, 15);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (9, 'VERTIMAG2.2', 322, 0, 2, 5, 700, 350, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 6, 12, 18);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (10, 'VERTIMAG2.3', 323, 0, 2, 5, 600, 300, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 7, 14, 21);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (11, 'VERTIMAG2.4', 324, 0, 2, 5, 500, 250, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 8, 16, 24);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (12, 'VERTIMAG2.5', 325, 0, 2, 5, 400, 200, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 9, 18, 27);

INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (13, 'VERTIMAG3.1', 341, 0, 2, 10, 800, 400, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 5, 10, 15);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (14, 'VERTIMAG3.2', 342, 0, 2, 10, 700, 350, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 6, 12, 18);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (15, 'VERTIMAG3.3', 343, 0, 2, 10, 600, 300, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 7, 14, 21);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (16, 'VERTIMAG3.4', 344, 0, 2, 10, 500, 250, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 8, 16, 24);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (17, 'VERTIMAG3.5', 345, 0, 2, 10, 400, 200, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 9, 18, 27);

INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (18, 'VERTIMAG4.1', 361, 0, 2, 13, 800, 400, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 5, 10, 15);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (19, 'VERTIMAG4.2', 362, 0, 2, 13, 700, 350, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 6, 12, 18);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (20, 'VERTIMAG4.3', 363, 0, 2, 13, 600, 300, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 7, 14, 21);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (21, 'VERTIMAG4.4', 364, 0, 2, 13, 500, 250, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 8, 16, 24);
INSERT INTO LoadingUnits (Id, Code, CellId, IsCellPairingFixed, CellPositionId, LoadingUnitTypeId, Height, Weight, LoadingUnitStatusId, Reference, AbcClassId, CreationDate, LastHandlingDate, InventoryDate, LastPickDate, LastStoreDate, InCycleCount, OutCycleCount, OtherCycleCount)
VALUES (22, 'VERTIMAG4.5', 365, 0, 2, 13, 400, 200, 'U', 'M', 'A', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', 9, 18, 27);
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
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (1, 1, 1, 0, 1, 1, 1, 2, 5, 's1s1s1', 's2s2s2', 'llllll', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (2, 1, 1, 0, 1, 1, 1, 2, 10, 's3s3s3', 's4s4s4', 'mmmmmm', '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');

INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (3, 3, 2, 0, 3, 1, 1, 2, 0, 's5s5s5', 's6s6s6', 'nnnnnn', 0, 0, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (4, 3, 2, 0, 3, 1, 1, 2, 5, 's7s7s7', 's8s8s8', 'pppppp', 0, 200, 200, 400, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (5, 3, 2, 0, 4, 1, 1, 2, 10, 's9s9s9', 's10s10s10', 'rrrrrr', 0, 600, 200, 50, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (6, 3, 2, 0, 4, 1, 1, 2, 20, 's11s11s11', 's12s12s12', 'tttttt', 200, 0, 400, 400, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (7, 3, 2, 0, 5, 1, 1, 2, 30, 's13s13s13', 's14s14s14', 'uuuuuu', 200, 400, 400, 250, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (8, 3, 2, 0, 5, 1, 1, 2, 40, 's15s15s15', 's16s16s16', 'vvvvvv', 600, 0, 200, 250, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (9, 3, 2, 0, 6, 1, 1, 2, 50, 's17s17s17', 's18s18s18', 'wwwwww', 600, 250, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (10, 3, 2, 0, 6, 1, 1, 2, 60, 's19s19s19', 's20s20s20', 'xxxxxx', 600, 450, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (11, 3, 2, 0, 6, 1, 1, 2, 70, 's21s21s21', 's22s22s22', 'yyyyyy', 800, 0, 200, 650, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (12, 3, 3, 0, 4, 1, 1, 2, 80, 's5s5s5', 's6s6s6', 'nnnnnn', 1000, 0, 500, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (13, 3, 3, 0, 5, 1, 1, 2, 90, 's7s7s7', 's8s8s8', 'pppppp', 1000, 200, 500, 400, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)
  VALUES (14, 3, 3, 0, 6, 1, 1, 2, 100, 's9s9s9', 's10s10s10', 'rrrrrr', 1000, 600, 500, 50, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO Compartments (Id, LoadingUnitId, CompartmentTypeId, IsItemPairingFixed, ItemId, MaterialStatusId, PackageTypeId, CompartmentStatusId, Stock, Sub1, Sub2, Lot, XPosition, YPosition, Width, Height, MaxCapacity, CreationDate, LastHandlingDate, InventoryDate, FirstStoreDate, LastStoreDate, LastPickDate)

  VALUES (15, 4, 2, 0, 3, 1, 1, 2, 40, 'sss111', 'sss222', 'qqqqqq', 0, 0, 200, 200, 100, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
SET IDENTITY_INSERT Compartments OFF;


-- Machines
INSERT INTO MachineTypes (Id, Description) VALUES ('T', 'Traslo');
INSERT INTO MachineTypes (Id, Description) VALUES ('S', 'Shuttle');
INSERT INTO MachineTypes (Id, Description) VALUES ('H', 'Handling');
INSERT INTO MachineTypes (Id, Description) VALUES ('L', 'LGV');
INSERT INTO MachineTypes (Id, Description) VALUES ('V', 'Vertimag');

SET IDENTITY_INSERT Machines ON;
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (1, @vrtmag_aisle1, 'V', 'Vertimag 1', 'so74jnh0vyenf', 'MachineVertimagM', 'VMAG/ver-2019/variant-XL/depth-65', 125);
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (2, @vrtmag_aisle2, 'V', 'Vertimag 2', 'msdy30yu76sb2', 'MachineVertimagXS', 'VMAG/ver-2018/variant-XS/depth-103', 286);
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (3, @vrtmag_aisle3, 'V', 'Vertimag 3', 'lwujg3ibg9h4j', 'MachineVertimagM', 'VMAG/ver-2018/variant-M/depth-84', 78);
INSERT INTO Machines (Id, AisleId, MachineTypeId, Nickname, RegistrationNumber, Image, Model, MovedLoadingUnitsCount) VALUES (4, @vrtmag_aisle4, 'V', 'Vertimag 4', '20fgn37o3nbe9', 'MachineVertimagXS', 'VMAG/ver-2019/variant-L/depth-84', 1904);
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
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (1, @BayTypes_Picking, 1, 'Single Pick Bay', @manual_area, null);
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (2, @BayTypes_Vertimag, 1, 'Vertimag Bay', @vrtmag_area, 1);
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (3, @BayTypes_Vertimag, 1, 'Vertimag Bay', @vrtmag_area, 2);
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (4, @BayTypes_Vertimag, 1, 'Vertimag Bay', @vrtmag_area, 3);
INSERT INTO Bays (Id, BayTypeId, LoadingUnitsBufferSize, Description, AreaId, MachineId) VALUES (5, @BayTypes_Vertimag, 1, 'Vertimag Bay', @vrtmag_area, 4);
SET IDENTITY_INSERT Bays OFF;

-- Operation Types
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


--Lists
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
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (@ItemList1_Id, 'List-1', @ItemListType_Pik, 'First List', 1, 'W', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (@ItemList2_Id, 'List-2', @ItemListType_Pik, 'Second List', 2, 'C', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (3,             'List-3', @ItemListType_Put, 'Third List', 3, 'W', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (4,             'List-4', @ItemListType_Put, 'Fourth List', 4, 'C', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (5,             'List-5', @ItemListType_Pik, 'Fifth List', 5, 'I', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (6,             'List-6', @ItemListType_Put, 'Sixth List', 6, 'S', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
INSERT INTO ItemLists (Id, Code, ItemListType, Description, Priority, Status, ShipmentUnitAssociated, CreationDate, LastModificationDate, FirstExecutionDate, ExecutionEndDate) VALUES (7,             'List-7', @ItemListType_Inv, 'Seventh List', 7, 'E', 1, '2018-11-16 12:33:14', '2017-10-05 14:16:00', '2017-05-01 09:57:00', '2016-06-06 15:20:00');
SET IDENTITY_INSERT ItemLists OFF;

--List Rows
SET IDENTITY_INSERT ItemListRows ON;
INSERT INTO ItemListRows (Id, ItemListId, Code, Priority, ItemId, MaterialStatusId, PackageTypeId, Lot, RequiredQuantity, DispatchedQuantity, Status) VALUES (1, @ItemList1_Id, 'Code 1', 1, 1, 1, 1, 'First Item'  , 50, 10, @ItemListStatus_Wait);
INSERT INTO ItemListRows (Id, ItemListId, Code, Priority, ItemId, MaterialStatusId, PackageTypeId, Lot, RequiredQuantity, DispatchedQuantity, Status) VALUES (2, @ItemList1_Id, 'Code 2', 1, 1, 1, 1, 'Second Item', 40, 20, @ItemListStatus_Wait);
INSERT INTO ItemListRows (Id, ItemListId, Code, Priority, ItemId, MaterialStatusId, PackageTypeId, Lot, RequiredQuantity, DispatchedQuantity, Status) VALUES (3, @ItemList1_Id, 'Code 3', 1, 1, 1, 1, 'Third Item'  , 25, 10, @ItemListStatus_Comp);
SET IDENTITY_INSERT ItemListRows OFF;


COMMIT;
