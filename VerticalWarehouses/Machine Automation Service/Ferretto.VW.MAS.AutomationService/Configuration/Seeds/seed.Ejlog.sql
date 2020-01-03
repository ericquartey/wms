UPDATE LoadingUnits SET GrossWeight = 500, CellId = 10, Status = 'InLocation' WHERE Id = 43;
UPDATE LoadingUnits SET GrossWeight = 500, CellId = 20, Status = 'InLocation' WHERE Id = 44;
UPDATE LoadingUnits SET GrossWeight = 500, CellId = 30, Status = 'InLocation' WHERE Id = 45;


UPDATE Cells SET Status = 'Occupied' WHERE Id = 10;
UPDATE Cells SET Status = 'Occupied' WHERE Id = 20;
UPDATE Cells SET Status = 'Occupied' WHERE Id = 30;
