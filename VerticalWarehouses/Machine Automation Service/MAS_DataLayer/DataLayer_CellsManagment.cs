using Ferretto.VW.Common_Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        #region Methods

        public List<Cell> GetCellList()
        {
            return inMemoryDataContext.Cells.ToList();
        }

        public ReturnMissionPosition GetFreeBlockPosition(decimal drawerHeight, int drawerId)
        {
            int cellSpacing = GetIntegerConfigurationValue(ConfigurationValueEnum.cellSpacing);

            int cellsNumber = (int)Math.Ceiling(drawerHeight / cellSpacing);

            // INFO Always take into account +1 drawer cell height, to avoid impacts between two next drowers
            cellsNumber += 1;

            // INFO Inserted a control to free a booked block, in the case the drawer is refused after the weight control
            FreeBlock inMemoryFreeBlockAlreadyBooked = inMemoryDataContext.FreeBlocks.FirstOrDefault(s => s.DrawerId >= drawerId);

            if (inMemoryFreeBlockAlreadyBooked != null)
            {
                inMemoryFreeBlockAlreadyBooked.BlockSize = 0;
                inMemoryFreeBlockAlreadyBooked.DrawerId = 0;

                inMemoryDataContext.SaveChanges();
            }

            FreeBlock inMemoryFreeBlockFirstByPriority = inMemoryDataContext.FreeBlocks.OrderBy(s => s.Priority)
                .FirstOrDefault(s => s.BlockSize >= cellsNumber);

            if (inMemoryFreeBlockFirstByPriority == null)
            {
                throw new InMemoryDataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKING_EXCEPTION);
            }

            // INFO Change the BookedCells number in the FreeBlock table
            inMemoryFreeBlockFirstByPriority.BookedCellsNumber = cellsNumber;
            inMemoryFreeBlockFirstByPriority.DrawerId = drawerId;
            inMemoryDataContext.SaveChanges();

            ReturnMissionPosition returnMissionPosition = new ReturnMissionPosition
            {
                ReturnCoord = inMemoryFreeBlockFirstByPriority.Coord,
                ReturnSide = inMemoryFreeBlockFirstByPriority.Side
            };

            return returnMissionPosition;
        }

        public void ReturnMissionEnded(int drawerId)
        {
            FreeBlock inMemoryFreeBlockSearchBookedCells = inMemoryDataContext.FreeBlocks.FirstOrDefault(s => s.BookedCellsNumber > 0 && s.DrawerId == drawerId);

            if (inMemoryFreeBlockSearchBookedCells == null)
            {
                throw new InMemoryDataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKED_EXCEPTION);
            }

            int filledStartCell = inMemoryFreeBlockSearchBookedCells.StartCell;
            int filledLastCell = filledStartCell + inMemoryFreeBlockSearchBookedCells.BookedCellsNumber * 2;

            for (int currentCell = filledStartCell; currentCell <= filledLastCell; currentCell += 2)
            {
                Cell inMemoryCellsSearchFilledCell = inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == currentCell);

                if (inMemoryCellsSearchFilledCell == null)
                {
                    throw new InMemoryDataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
                }

                inMemoryCellsSearchFilledCell.Status = Status.Occupied;
            }

            // INFO Run the Free Block table calculation after the update
            CreateFreeBlockTable();
        }

        public bool SetCellList(List<Cell> listCells)
        {
            bool setCellList = false;

            if (listCells != null)
            {
                setCellList = true;

                foreach (Cell cell in listCells)
                {
                    Cell inMemoryCellCurrentValue =
                        inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == cell.CellId);

                    if (inMemoryCellCurrentValue != null)
                    {
                        inMemoryCellCurrentValue.Coord = cell.Coord;
                        inMemoryCellCurrentValue.Priority = cell.Priority;
                        inMemoryCellCurrentValue.Side = cell.Side;
                        inMemoryCellCurrentValue.Status = cell.Status;

                        inMemoryDataContext.SaveChanges();
                    }
                    else
                        throw new ArgumentNullException();
                }
            }

            return setCellList;
        }

        private void CreateFreeBlockTable()
        {
            bool cellTablePopulated = false;

            int freeBlockCounter = 1;

            int cellCounterEven = 0;
            int cellCounterOdd = 0;

            FreeBlock evenFreeBlock = new FreeBlock();
            FreeBlock oddFreeBlock = new FreeBlock();

            int evenCellBeforePriority = -1;
            int oddCellBeforePriority = -1;

            // INFO Remove all the records from the FreeBlocks table
            inMemoryDataContext.FreeBlocks.RemoveRange(inMemoryDataContext.FreeBlocks);
            inMemoryDataContext.SaveChanges();

            foreach (Cell cell in inMemoryDataContext.Cells.OrderBy(cell => cell.CellId))
            {
                cellTablePopulated = true;

                if (cell.Side == Side.FrontEven)
                {
                    if (cellCounterEven != 0 && (cell.Status == Status.Free || cell.Status == Status.Disabled) && evenCellBeforePriority < cell.Priority)
                    {
                        cellCounterEven++;
                    }

                    if (cell.Status == Status.Free && cellCounterEven == 0 /*&& evenCellBeforePriority < cell.Priority*/)
                    {
                        evenFreeBlock.StartCell = cell.CellId;
                        evenFreeBlock.FreeBlockId = freeBlockCounter;
                        evenFreeBlock.Priority = cell.Priority;
                        evenFreeBlock.Coord = cell.Coord;
                        evenFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterEven++;
                    }

                    if (cellCounterEven != 0 && (cell.Status == Status.Occupied || cell.Status == Status.Unusable || evenCellBeforePriority > cell.Priority))
                    {
                        evenFreeBlock.BlockSize = cellCounterEven;
                        evenFreeBlock.BookedCellsNumber = 0;

                        cellCounterEven = 0;
                        inMemoryDataContext.FreeBlocks.Add(evenFreeBlock);
                    }

                    // INFO Saving the Priority before
                    evenCellBeforePriority = cell.Priority;
                }
                else
                {
                    if (cellCounterOdd != 0 && (cell.Status == Status.Free || cell.Status == Status.Disabled) && oddCellBeforePriority < cell.Priority)
                    {
                        cellCounterOdd++;
                    }

                    if (cell.Status == Status.Free && cellCounterOdd == 0 /*&& oddCellBeforePriority < cell.Priority*/)
                    {
                        oddFreeBlock.StartCell = cell.CellId;
                        oddFreeBlock.FreeBlockId = freeBlockCounter;
                        oddFreeBlock.Priority = cell.Priority;
                        oddFreeBlock.Coord = cell.Coord;
                        oddFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterOdd++;
                    }

                    if (cellCounterOdd != 0 && (cell.Status == Status.Occupied || cell.Status == Status.Unusable || oddCellBeforePriority > cell.Priority))
                    {
                        oddFreeBlock.BlockSize = cellCounterOdd;
                        oddFreeBlock.BookedCellsNumber = 0;

                        cellCounterOdd = 0;
                        inMemoryDataContext.FreeBlocks.Add(oddFreeBlock);
                    }

                    // INFO Saving the Priority before
                    oddCellBeforePriority = cell.Priority;
                }
            }

            if (!cellTablePopulated)
            {
                throw new InMemoryDataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
            }

            if (!inMemoryDataContext.FreeBlocks.Any())
            {
                throw new InMemoryDataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKING_EXCEPTION);
            }
        }

        #endregion
    }
}
