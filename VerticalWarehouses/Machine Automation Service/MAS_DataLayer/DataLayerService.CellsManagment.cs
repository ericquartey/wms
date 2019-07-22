using System;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ICellManagmentDataLayer
    {
        #region Methods

        public CellStatisticsSummary GetCellStatistics()
        {
            var totalCells = this.primaryDataContext.Cells.Count();
            var cellStatusStatistics = this.primaryDataContext.Cells
                .GroupBy(c => c.Status)
                .Select(g =>
                    new CellStatusStatistics
                    {
                        Status = g.Key,
                        TotalFrontCells = g.Count(c => c.Side == CellSide.Front),
                        TotalBackCells = g.Count(c => c.Side == CellSide.Back),
                        RatioFrontCells = g.Count(c => c.Side == CellSide.Front) / (double)totalCells,
                        RatioBackCells = g.Count(c => c.Side == CellSide.Back) / (double)totalCells,
                    });

            var totalOccupiedOrUnusableCells =
                this.primaryDataContext.Cells.Count(c => c.Status == CellStatus.Occupied || c.Status == CellStatus.Unusable);

            var cellStatistics = new CellStatisticsSummary
            {
                CellStatusStatistics = cellStatusStatistics,
                TotalCells = totalCells,
                TotalFrontCells = this.primaryDataContext.Cells.Count(c => c.Side == CellSide.Front),
                TotalBackCells = this.primaryDataContext.Cells.Count(c => c.Side == CellSide.Back),
                CellOccupationPercentage = totalOccupiedOrUnusableCells * 100.0 / totalCells,
            };

            return cellStatistics;
        }

        // INFO Method used when a drawer backs in the magazine from bay (return mission).
        public async Task<LoadingUnitPosition> GetFreeBlockPositionAsync(decimal loadingUnitHeight, int loadingUnitId)
        {
            var cellSpacing = 1; //this.GetIntegerConfigurationValue((long)ConfigurationValueEnum.CellSpacing, (long)ConfigurationCategory.GeneralInfoEnum);

            var cellsNumber = (int)Math.Ceiling(loadingUnitHeight / cellSpacing);

            // INFO Always take into account +1 drawer cell height, to avoid impacts between two next drowers
            cellsNumber += 1;

            // INFO Inserted a control to free a booked block, in the case the drawer is refused after the weight control
            var inMemoryFreeBlockAlreadyBooked = this.primaryDataContext.FreeBlocks.FirstOrDefault(s => s.LoadingUnitId == loadingUnitId);

            if (inMemoryFreeBlockAlreadyBooked != null)
            {
                inMemoryFreeBlockAlreadyBooked.BlockSize = 0;
                inMemoryFreeBlockAlreadyBooked.LoadingUnitId = 0;

                await this.primaryDataContext.SaveChangesAsync();
            }

            var inMemoryFreeBlockFirstByPriority = this.primaryDataContext.FreeBlocks.OrderBy(s => s.Priority).FirstOrDefault(s => s.BlockSize >= cellsNumber);

            if (inMemoryFreeBlockFirstByPriority == null)
            {
                throw new DataLayerException(DataLayerExceptionCode.NoFreeBlockBookingException);
            }

            // INFO Change the BookedCells number in the FreeBlock table
            inMemoryFreeBlockFirstByPriority.BookedCellsNumber = cellsNumber;
            inMemoryFreeBlockFirstByPriority.LoadingUnitId = loadingUnitId;

            // INFO Change the LoadingUnit height with the new value, in the LoadingUnit table
            var inMemoryLoadingUnit = this.primaryDataContext.LoadingUnits.FirstOrDefault(s => s.Id == loadingUnitId);
            inMemoryLoadingUnit.Height = loadingUnitHeight;

            await this.primaryDataContext.SaveChangesAsync();

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
            var cell = this.primaryDataContext.Cells.FirstOrDefault(s => s.Id == cellId);
            if (cell == null)
            {
                throw new DataLayerException(DataLayerExceptionCode.CellNotFoundException);
            }

            return new LoadingUnitPosition
            {
                LoadingUnitCoord = cell.Coord,
                LoadingUnitSide = cell.Side
            };
        }

        // INFO Method called when a drawer backs in the magazine and it occupies some cells
        public async void SetReturnLoadingUnitInLocation(int loadingUnitId)
        {
            // INFO Search in the FreeBlock table the booked cells for the drawer
            var firstFreeBlock = this.primaryDataContext
                .FreeBlocks
                .FirstOrDefault(s =>
                    s.BookedCellsNumber > 0
                    &&
                    s.LoadingUnitId == loadingUnitId);

            if (firstFreeBlock == null)
            {
                throw new DataLayerException(DataLayerExceptionCode.NoFreeBlockBookedException);
            }

            var loadintUnit = this.primaryDataContext
                .LoadingUnits
                .SingleOrDefault(l => l.Id == loadingUnitId);

            if (loadintUnit == null)
            {
                throw new DataLayerException(DataLayerExceptionCode.LoadingUnitNotFoundException);
            }

            var filledStartCell = firstFreeBlock.StartCell;
            var filledLastCell = filledStartCell + (firstFreeBlock.BookedCellsNumber * 2);

            for (var currentCellIndex = filledStartCell; currentCellIndex <= filledLastCell; currentCellIndex += 2)
            {
                var currentCell = this.primaryDataContext.Cells.FirstOrDefault(c => c.Id == currentCellIndex);
                if (currentCell == null)
                {
                    throw new DataLayerException(DataLayerExceptionCode.CellNotFoundException);
                }

                currentCell.WorkingStatus = CellStatus.Occupied;
            }

            // INFO Update the LoadingUnit table when the LoadingUnit is in location
            loadintUnit.CellId = filledStartCell;
            loadintUnit.Status = LoadingUnitStatus.InLocation;

            await this.primaryDataContext.SaveChangesAsync();

            // INFO Run the Free Block table calculation after the update
            this.CreateFreeBlockTable();
        }

        // INFO Procedure called when a drawer frees some cells in a first type mission from cells to bay.
        public async void SetWithdrawalLoadingUnitFromLocation(int loadingUnitId)
        {
            var loadingUnit = this.primaryDataContext.LoadingUnits
                .Include(l => l.Cell)
                .SingleOrDefault(l => l.Id == loadingUnitId);

            if (loadingUnit == null)
            {
                throw new DataLayerException(DataLayerExceptionCode.LoadingUnitNotFoundException);
            }

            // INFO Copies a coloumn in another coloumn
            loadingUnit.Cell.WorkingStatus = loadingUnit.Cell.Status;
            loadingUnit.CellId = null;

            // INFO Update the LoadingUnit table
            loadingUnit.Status = LoadingUnitStatus.OnMovementToBay;

            await this.primaryDataContext.SaveChangesAsync();

            this.CreateFreeBlockTable();
        }

        // INFO Procedure to create the Free Blocks table
        private async void CreateFreeBlockTable()
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
            await this.primaryDataContext.SaveChangesAsync();

            foreach (var cell in this.primaryDataContext.Cells.OrderBy(cell => cell.Id))
            {
                cellTablePopulated = true;

                if (cell.Side == CellSide.Front)
                {
                    if (cellCounterEven != 0
                        &&
                        (cell.WorkingStatus == CellStatus.Free || cell.WorkingStatus == CellStatus.Disabled)
                        &&
                        evenCellBeforePriority < cell.Priority)
                    {
                        cellCounterEven++;
                    }

                    if (cell.WorkingStatus == CellStatus.Free && cellCounterEven == 0)
                    {
                        evenFreeBlock.StartCell = cell.Id;
                        evenFreeBlock.FreeBlockId = freeBlockCounter;
                        evenFreeBlock.Priority = cell.Priority;
                        evenFreeBlock.Coord = cell.Coord;
                        evenFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterEven++;
                    }

                    if (cellCounterEven != 0
                        &&
                        (cell.WorkingStatus == CellStatus.Occupied || cell.WorkingStatus == CellStatus.Unusable || evenCellBeforePriority > cell.Priority))
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
                    if (cellCounterOdd != 0 && (cell.WorkingStatus == CellStatus.Free || cell.WorkingStatus == CellStatus.Disabled) && oddCellBeforePriority < cell.Priority)
                    {
                        cellCounterOdd++;
                    }

                    if (cell.WorkingStatus == CellStatus.Free && cellCounterOdd == 0)
                    {
                        oddFreeBlock.StartCell = cell.Id;
                        oddFreeBlock.FreeBlockId = freeBlockCounter;
                        oddFreeBlock.Priority = cell.Priority;
                        oddFreeBlock.Coord = cell.Coord;
                        oddFreeBlock.Side = cell.Side;

                        freeBlockCounter++;
                        cellCounterOdd++;
                    }

                    if (cellCounterOdd != 0 && (cell.WorkingStatus == CellStatus.Occupied || cell.WorkingStatus == CellStatus.Unusable || oddCellBeforePriority > cell.Priority))
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
                throw new DataLayerException(DataLayerExceptionCode.CellNotFoundException);
            }

            if (!this.primaryDataContext.FreeBlocks.Any())
            {
                throw new DataLayerException(DataLayerExceptionCode.NoFreeBlockBookingException);
            }
        }

        #endregion
    }
}
