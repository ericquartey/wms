UPDATE LoadingUnits SET GrossWeight = 500, Height = 200 WHERE Id = 43;
UPDATE LoadingUnits SET GrossWeight = 500, Height = 200, CellId = 20, Status = 'InLocation' WHERE Id = 44;
UPDATE LoadingUnits SET GrossWeight = 500, Height = 200, CellId = 30, Status = 'InLocation' WHERE Id = 45;
UPDATE LoadingUnits SET GrossWeight = 500, Height = 200, CellId = 40, Status = 'InLocation' WHERE Id = 46;


UPDATE Cells SET IsFree = 0  WHERE Id = 20 or Id = 30 or Id = 40;
