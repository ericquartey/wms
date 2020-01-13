UPDATE LoadingUnits SET GrossWeight = 500, Height = 15, CellId = 10, Status = 'InLocation' WHERE Id = 43;
UPDATE LoadingUnits SET GrossWeight = 500, Height = 15, CellId = 20, Status = 'InLocation' WHERE Id = 44;
UPDATE LoadingUnits SET GrossWeight = 500, Height = 15, CellId = 30, Status = 'InLocation' WHERE Id = 45;
UPDATE LoadingUnits SET GrossWeight = 500, Height = 15, CellId = 40, Status = 'InLocation' WHERE Id = 46;


UPDATE Cells SET IsFree = 0  WHERE Id = 10  or Id = 20 or Id = 30 or Id = 40;
