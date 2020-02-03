UPDATE Elevators SET LoadingUnitId = NULL;

-- front
UPDATE LoadingUnits SET CellId = 114,   GrossWeight = 129 WHERE Id = 1;
UPDATE LoadingUnits SET CellId = 40,   GrossWeight = 436 WHERE Id = 2;
UPDATE LoadingUnits SET CellId = 73,   GrossWeight = 620 WHERE Id = 3;

UPDATE Cells SET IsFree = 0 WHERE Id = 114  or Id = 40 or Id = 73;

