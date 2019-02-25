using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        #region Methods

        public List<Cell> GetCellList()
        {
            return this.inMemoryDataContext.Cells.ToList();
        }

        public ReturnMissionPosition GetFreeBlockPosition(decimal drawerHeight)
        {
            var cellSpacing = this.GetIntegerConfigurationValue(ConfigurationValueEnum.cellSpacing);

            var cellsNumber = (int)Math.Ceiling(drawerHeight / cellSpacing);

            // INFO Always take into account +1 drawer cell height, to avoid impacts between two next drowers
            cellsNumber += 1;

            var inMemoryFreeBlockFirstByPriority = this.inMemoryDataContext.FreeBlocks.OrderBy(s => s.Priority).FirstOrDefault(s => s.BlockSize >= cellsNumber);

            if (inMemoryFreeBlockFirstByPriority == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_EXCEPTION);
            }

            // INFO Change the BookedCells number in the FreeBlock table
            inMemoryFreeBlockFirstByPriority.BookedCellsNumber = cellsNumber;
            this.inMemoryDataContext.SaveChanges();

            var returnMissionPosition = new ReturnMissionPosition();

            returnMissionPosition.ReturnCoord = inMemoryFreeBlockFirstByPriority.Coord;
            returnMissionPosition.ReturnSide = inMemoryFreeBlockFirstByPriority.Side;

            return returnMissionPosition;
        }

        //public bool ReturnMissionEnded()
        //{
        //    var cellTableUpdated = true;

        //    var inMemoryFreeBlockSearchBookedCells = this.inMemoryDataContext.FreeBlocks.FirstOrDefault(s => s.BookedCellsNumber > 0);

        //    int bookedCellsNumber = inMemoryFreeBlockSearchBookedCells.BookedCellsNumber;
        //    int filledCell = inMemoryFreeBlockSearchBookedCells.StartCell;

        //    for (int i = 0 ; i < bookedCellsNumber ; i++)
        //    {
        //        var inMemoryCellsSearchFilledCell = this.inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == filledCell);
        //        inMemoryCellsSearchFilledCell.Status = Status.Occupied;
        //    }

        //    return cellTableUpdated;
        //}

        public bool SetCellList(List<Cell> listCells)
        {
            var setCellList = false;

            if (listCells != null)
            {
                setCellList = true;

                foreach (var cell in listCells)
                {
                    var inMemoryCellCurrentValue = this.inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == cell.CellId);

                    if (inMemoryCellCurrentValue != null)
                    {
                        inMemoryCellCurrentValue.Coord = cell.Coord;
                        inMemoryCellCurrentValue.Priority = cell.Priority;
                        inMemoryCellCurrentValue.Side = cell.Side;
                        inMemoryCellCurrentValue.Status = cell.Status;

                        this.inMemoryDataContext.SaveChanges();
                    }
                    else
                    {
                        throw new ArgumentNullException();
                    }
                }
            }

            return setCellList;
        }

        private void CreateFreeBlockTable()
        {
            var cellTablePopulated = false;

            var freeBlockCounter = 1;

            var cellCounterEven = 0;
            var cellCounterOdd = 0;

            var evenFreeBlock = new FreeBlock();
            var oddFreeBlock = new FreeBlock();

            // INFO Remove all the records from the FreeBlocks table
            this.inMemoryDataContext.FreeBlocks.RemoveRange(this.inMemoryDataContext.FreeBlocks);
            this.inMemoryDataContext.SaveChanges();

            foreach (var cell in this.inMemoryDataContext.Cells.OrderBy(cell => cell.CellId))
            {
                cellTablePopulated = true;

                if (cell.Side == Side.FrontEven)
                {
                    if (cellCounterEven != 0 && (cell.Status == Status.Free || cell.Status == Status.Disabled))
                    {
                        cellCounterEven++;
                    }

                    if (cell.Status == Status.Free && cellCounterEven == 0)
                    {
                        evenFreeBlock.StartCell = cell.CellId;
                        evenFreeBlock.FreeBlockId = freeBlockCounter;
                        evenFreeBlock.Priority = freeBlockCounter;
                        evenFreeBlock.Coord = cell.Coord;
                        evenFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterEven++;
                    }

                    if (cellCounterEven != 0 && (cell.Status == Status.Occupied || cell.Status == Status.Unusable))
                    {
                        evenFreeBlock.BlockSize = cellCounterEven;
                        evenFreeBlock.BookedCellsNumber = 0;

                        cellCounterEven = 0;
                        this.inMemoryDataContext.FreeBlocks.Add(evenFreeBlock);
                    }
                }
                else
                {
                    if (cellCounterOdd != 0 && (cell.Status == Status.Free || cell.Status == Status.Disabled))
                    {
                        cellCounterOdd++;
                    }

                    if (cell.Status == Status.Free && cellCounterOdd == 0)
                    {
                        oddFreeBlock.StartCell = cell.CellId;
                        oddFreeBlock.FreeBlockId = freeBlockCounter;
                        oddFreeBlock.Priority = freeBlockCounter;
                        oddFreeBlock.Coord = cell.Coord;
                        oddFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterOdd++;
                    }

                    if (cellCounterOdd != 0 && (cell.Status == Status.Occupied || cell.Status == Status.Unusable))
                    {
                        oddFreeBlock.BlockSize = cellCounterOdd;
                        oddFreeBlock.BookedCellsNumber = 0;

                        cellCounterOdd = 0;
                        this.inMemoryDataContext.FreeBlocks.Add(oddFreeBlock);
                    }
                }
            }

            if (!cellTablePopulated)
            {
                throw new DataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
            }

            if (!this.inMemoryDataContext.FreeBlocks.Any())
            {
                throw new DataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_EXCEPTION);
            }
        }

        #endregion
    }
}
