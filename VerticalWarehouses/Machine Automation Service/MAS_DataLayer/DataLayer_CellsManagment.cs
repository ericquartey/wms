using System;
using System.Linq;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayerCellManagment
    {
        // TEMP Maybe obsolete
        //public List<Cell> GetCellList()
        //{
        //    return this.inMemoryDataContext.Cells.ToList();
        //}

        #region Methods

        // INFO Method used when a drawer backs in the magazine from bay (return mission).
        public LoadingUnitPosition GetFreeBlockPosition(decimal loadingUnitHeight, int loadingUnitId)
        {
            var cellSpacing = 1;//this.GetIntegerConfigurationValue((long)ConfigurationValueEnum.CellSpacing, (long)ConfigurationCategory.GeneralInfoEnum);

            var cellsNumber = (int)Math.Ceiling(loadingUnitHeight / cellSpacing);

            // INFO Always take into account +1 drawer cell height, to avoid impacts between two next drowers
            cellsNumber += 1;

            // INFO Inserted a control to free a booked block, in the case the drawer is refused after the weight control
            var inMemoryFreeBlockAlreadyBooked = this.primaryDataContext.FreeBlocks.FirstOrDefault(s => s.LoadingUnitId == loadingUnitId);

            if (inMemoryFreeBlockAlreadyBooked != null)
            {
                inMemoryFreeBlockAlreadyBooked.BlockSize = 0;
                inMemoryFreeBlockAlreadyBooked.LoadingUnitId = 0;

                this.primaryDataContext.SaveChanges();
            }

            var inMemoryFreeBlockFirstByPriority = this.primaryDataContext.FreeBlocks.OrderBy(s => s.Priority).FirstOrDefault(s => s.BlockSize >= cellsNumber);

            if (inMemoryFreeBlockFirstByPriority == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKING_EXCEPTION);
            }

            // INFO Change the BookedCells number in the FreeBlock table
            inMemoryFreeBlockFirstByPriority.BookedCellsNumber = cellsNumber;
            inMemoryFreeBlockFirstByPriority.LoadingUnitId = loadingUnitId;

            // INFO Change the LoadingUnit height with the new value, in the LoadingUnit table
            var inMemoryLoadingUnit = this.primaryDataContext.LoadingUnits.FirstOrDefault(s => s.LoadingUnitId == loadingUnitId);
            inMemoryLoadingUnit.Height = loadingUnitHeight;

            this.primaryDataContext.SaveChanges();

            var returnLoadingUnitPosition = new LoadingUnitPosition
            {
                LoadingUnitCoord = inMemoryFreeBlockFirstByPriority.Coord,
                LoadingUnitSide = inMemoryFreeBlockFirstByPriority.Side
            };

            return returnLoadingUnitPosition;
        }

        // INFO The method returns to the machine manager the position to take a drawer for mission from the WMS
        public LoadingUnitPosition GetLoadingUnitPosition(int cellId)
        {
            var loadingUnitPosition = new LoadingUnitPosition();

            var inMemoryCellPosition = this.primaryDataContext.Cells.FirstOrDefault(s => s.CellId == cellId);

            if (inMemoryCellPosition == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
            }

            loadingUnitPosition.LoadingUnitCoord = inMemoryCellPosition.Coord;
            loadingUnitPosition.LoadingUnitSide = inMemoryCellPosition.Side;

            return loadingUnitPosition;
        }

        // INFO Method called when a drawer backs in the magazine and it occupies some cells
        public void SetReturnLoadingUnitInLocation(int loadingUnitId)
        {
            // INFO Search in the FreeBlock table the booked cells for the drawer
            var inMemoryFreeBlockSearchBookedCells = this.primaryDataContext.FreeBlocks.FirstOrDefault(s => s.BookedCellsNumber > 0 && s.LoadingUnitId == loadingUnitId);

            if (inMemoryFreeBlockSearchBookedCells == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKED_EXCEPTION);
            }

            var filledStartCell = inMemoryFreeBlockSearchBookedCells.StartCell;
            var filledLastCell = filledStartCell + inMemoryFreeBlockSearchBookedCells.BookedCellsNumber * 2;

            for (var currentCell = filledStartCell; currentCell <= filledLastCell; currentCell += 2)
            {
                var inMemoryCellsSearchFilledCell = this.primaryDataContext.Cells.FirstOrDefault(s => s.CellId == currentCell);

                if (inMemoryCellsSearchFilledCell == null)
                {
                    throw new DataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
                }

                inMemoryCellsSearchFilledCell.WorkingStatus = Status.Occupied;
                inMemoryCellsSearchFilledCell.LoadingUnitId = loadingUnitId;
            }

            // INFO Update the LoadingUnit table when the LoadingUnit is in location
            var loadingUnitOnMovement = this.primaryDataContext.LoadingUnits.FirstOrDefault(s => s.LoadingUnitId == loadingUnitId);
            loadingUnitOnMovement.CellPosition = filledStartCell;
            loadingUnitOnMovement.Status = LoadingUnitStatus.InLocation;

            this.primaryDataContext.SaveChanges();

            // INFO Run the Free Block table calculation after the update
            this.CreateFreeBlockTable();
        }

        // TEMP Maybe obsolete
        //public bool SetCellList(List<Cell> listCells)
        //{
        //    var setCellList = false;

        //    if (listCells != null)
        //    {
        //        setCellList = true;

        //        foreach (var cell in listCells)
        //        {
        //            var inMemoryCellCurrentValue = this.inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == cell.CellId);

        //            if (inMemoryCellCurrentValue != null)
        //            {
        //                inMemoryCellCurrentValue.Coord = cell.Coord;
        //                inMemoryCellCurrentValue.Priority = cell.Priority;
        //                inMemoryCellCurrentValue.Side = cell.Side;
        //                inMemoryCellCurrentValue.Status = cell.Status;

        //                this.inMemoryDataContext.SaveChanges();
        //            }
        //            else
        //            {
        //                throw new ArgumentNullException();
        //            }
        //        }
        //    }

        //    return setCellList;
        //}

        // INFO Procedure called when a drawer frees some cells in a first type mission from cells to bay.
        public void SetWithdrawalLoadingUnitFromLocation(int loadingUnitId)
        {
            var freeCells = this.primaryDataContext.Cells.FirstOrDefault(s => s.LoadingUnitId == loadingUnitId);

            if (freeCells == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
            }

            // INFO Copies a coloumn in another coloumn
            freeCells.WorkingStatus = freeCells.Status;
            freeCells.LoadingUnitId = 0;

            // INFO Update the LoadingUnit table
            var loadingUnitOnMovement = this.primaryDataContext.LoadingUnits.FirstOrDefault(s => s.LoadingUnitId == loadingUnitId);
            loadingUnitOnMovement.CellPosition = 0;
            loadingUnitOnMovement.Status = LoadingUnitStatus.OnMovementToBay;

            this.primaryDataContext.SaveChanges();

            this.CreateFreeBlockTable();
        }

        // INFO Procedure to create the Free Blocks table
        private void CreateFreeBlockTable()
        {
            var cellTablePopulated = false;

            var freeBlockCounter = 1;

            var cellCounterEven = 0;
            var cellCounterOdd = 0;

            var evenFreeBlock = new FreeBlock();
            var oddFreeBlock = new FreeBlock();

            var evenCellBeforePriority = -1;
            var oddCellBeforePriority = -1;

            // INFO Remove all the records from the FreeBlocks table
            this.primaryDataContext.FreeBlocks.RemoveRange(this.primaryDataContext.FreeBlocks);
            this.primaryDataContext.SaveChanges();

            foreach (var cell in this.primaryDataContext.Cells.OrderBy(cell => cell.CellId))
            {
                cellTablePopulated = true;

                if (cell.Side == Side.FrontEven)
                {
                    if (cellCounterEven != 0 && (cell.WorkingStatus == Status.Free || cell.WorkingStatus == Status.Disabled) && evenCellBeforePriority < cell.Priority)
                    {
                        cellCounterEven++;
                    }

                    if (cell.WorkingStatus == Status.Free && cellCounterEven == 0)
                    {
                        evenFreeBlock.StartCell = cell.CellId;
                        evenFreeBlock.FreeBlockId = freeBlockCounter;
                        evenFreeBlock.Priority = cell.Priority;
                        evenFreeBlock.Coord = cell.Coord;
                        evenFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterEven++;
                    }

                    if (cellCounterEven != 0 && (cell.WorkingStatus == Status.Occupied || cell.WorkingStatus == Status.Unusable || evenCellBeforePriority > cell.Priority))
                    {
                        evenFreeBlock.BlockSize = cellCounterEven;
                        evenFreeBlock.BookedCellsNumber = 0;

                        cellCounterEven = 0;
                        this.primaryDataContext.FreeBlocks.Add(evenFreeBlock);
                    }

                    // INFO Saving the Priority before
                    evenCellBeforePriority = cell.Priority;
                }
                else
                {
                    if (cellCounterOdd != 0 && (cell.WorkingStatus == Status.Free || cell.WorkingStatus == Status.Disabled) && oddCellBeforePriority < cell.Priority)
                    {
                        cellCounterOdd++;
                    }

                    if (cell.WorkingStatus == Status.Free && cellCounterOdd == 0)
                    {
                        oddFreeBlock.StartCell = cell.CellId;
                        oddFreeBlock.FreeBlockId = freeBlockCounter;
                        oddFreeBlock.Priority = cell.Priority;
                        oddFreeBlock.Coord = cell.Coord;
                        oddFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterOdd++;
                    }

                    if (cellCounterOdd != 0 && (cell.WorkingStatus == Status.Occupied || cell.WorkingStatus == Status.Unusable || oddCellBeforePriority > cell.Priority))
                    {
                        oddFreeBlock.BlockSize = cellCounterOdd;
                        oddFreeBlock.BookedCellsNumber = 0;

                        cellCounterOdd = 0;
                        this.primaryDataContext.FreeBlocks.Add(oddFreeBlock);
                    }

                    // INFO Saving the Priority before
                    oddCellBeforePriority = cell.Priority;
                }
            }

            if (!cellTablePopulated)
            {
                throw new DataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
            }

            if (!this.primaryDataContext.FreeBlocks.Any())
            {
                throw new DataLayerException(DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKING_EXCEPTION);
            }
        }

        #endregion
    }
}
