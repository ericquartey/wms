UPDATE LoadingUnits SET GrossWeight = 500, CellId = 10 WHERE Id = 43;
UPDATE LoadingUnits SET GrossWeight = 500, CellId = 20 WHERE Id = 44;
UPDATE LoadingUnits SET GrossWeight = 500, CellId = 30 WHERE Id = 45;


UPDATE Cells SET IsFree = 0  WHERE Id = 10;
UPDATE Cells SET IsFree = 0  WHERE Id = 20;
UPDATE Cells SET IsFree = 0  WHERE Id = 30;
