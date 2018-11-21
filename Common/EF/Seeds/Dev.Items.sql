SET QUOTED_IDENTIFIER ON

DECLARE
    @item_management_fifo char(1) = 'F',
    @item_management_vol  char(1) = 'V'

SET IDENTITY_INSERT Items ON;

DECLARE @count INT = 1;
WHILE @count < 50000
BEGIN

    IF @count % 2 = 0
    BEGIN
        INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId)
               VALUES (@count, CONCAT('Code_', @count) ,CONCAT('Description_', @count),'B','KG',@item_management_fifo, 1);

    END
    ELSE
    BEGIN
        INSERT INTO Items (Id, Code, Description, AbcClassId, MeasureUnitId, ManagementType, ItemCategoryId)
               VALUES (@count, CONCAT('Code_', @count) ,CONCAT('Description_', @count),'A','PZ',@item_management_vol, 1);

    END
   
   SET @count = @count + 1;
END;

SET IDENTITY_INSERT Items OFF;
