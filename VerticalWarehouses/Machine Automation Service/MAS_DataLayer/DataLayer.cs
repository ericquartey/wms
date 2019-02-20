using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Microsoft.EntityFrameworkCore;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer, IWriteLogService
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

        private readonly IEventAggregator eventAggregator;

        private readonly DataLayerContext inMemoryDataContext;

        #endregion

        #region Constructors

        public DataLayer(string connectionString, DataLayerContext inMemoryDataContext, IEventAggregator eventAggregator)
        {
            if (inMemoryDataContext == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.DATALAYER_CONTEXT_EXCEPTION);
            }

            if (eventAggregator == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.EVENTAGGREGATOR_EXCEPTION);
            }

            this.inMemoryDataContext = inMemoryDataContext;

            this.eventAggregator = eventAggregator;

            using (var initialContext = new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options))
            {
                initialContext.Database.Migrate();

                if (!initialContext.ConfigurationValues.Any())
                {
                    //TODO reovery database from permanent storage
                }

                foreach (var configurationValue in initialContext.ConfigurationValues)
                {
                    this.inMemoryDataContext.ConfigurationValues.Add(configurationValue);
                }

                this.inMemoryDataContext.SaveChanges();
            }

            // The old WriteLogService
            var webApiCommandEvent = eventAggregator.GetEvent<WebAPI_CommandEvent>();

            webApiCommandEvent.Subscribe(this.LogWriting);
        }

        #endregion

        #region Methods

        public List<Cell> GetCellList()
        {
            var listCells = new List<Cell>();
            foreach (var cell in this.inMemoryDataContext.Cells)
            {
                listCells.Add(cell);
            }
            return listCells;
        }

        public ReturnMissionPosition GetFreeBlockPosition(int drawerHeight)
        {
            var cellSpacing = this.GetIntegerConfigurationValue(ConfigurationValueEnum.cellSpicing);

            // TEMP Drawer height conversion to the necessary cells number Ceiling to round a double to the upper integer
            var cellNumber = (int)Math.Ceiling((decimal)drawerHeight / cellSpacing);

            var cellEven = new Cell();
            var cellOdd = new Cell();

            var cellCounterEven = 0;
            var cellCounterOdd = 0;

            var returnMissionPosition = new ReturnMissionPosition();

            foreach (var cell in this.inMemoryDataContext.Cells.OrderBy(cell => cell.CellId))
            {
                if (cell.Side == Side.FrontEven)
                {
                    if (cell.Status == Status.Free && cellCounterEven == 0)
                    {
                        cellEven = cell;
                        cellCounterEven++;
                    }

                    if (cell.Status == Status.Free && cellCounterEven != 0)
                    {
                        cellCounterEven++;
                    }

                    if (cell.Status == Status.Disabled && cellCounterEven != 0)
                    {
                        cellCounterEven++;
                    }

                    if (cell.Status != Status.Occupied || cell.Status == Status.Unusable)
                    {
                        cellCounterEven = 0;
                    }
                }
                else
                {
                    if (cell.Status == Status.Free && cellCounterOdd == 0)
                    {
                        cellOdd = cell;
                        cellCounterOdd++;
                    }

                    if (cell.Status == Status.Free && cellCounterOdd != 0)
                    {
                        cellCounterOdd++;
                    }

                    if (cell.Status == Status.Disabled && cellCounterOdd != 0)
                    {
                        cellCounterOdd++;
                    }

                    if (cell.Status != Status.Occupied || cell.Status == Status.Unusable)
                    {
                        cellCounterOdd = 0;
                    }
                }

                // INFO - if the block is high or higher the drawer we end to search for the block
                if (cellCounterEven >= cellNumber)
                {
                    returnMissionPosition.ReturnCoord = cellEven.Coord;
                    returnMissionPosition.ReturnSide = cellEven.Side;

                    break;
                }

                if (cellCounterOdd >= cellNumber)
                {
                    returnMissionPosition.ReturnCoord = cellOdd.Coord;
                    returnMissionPosition.ReturnSide = cellOdd.Side;

                    break;
                }
            }

            return returnMissionPosition;
        }

        public bool LogWriting(string logMessage)
        {
            var updateOperation = true;

            try
            {
                this.inMemoryDataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
                this.inMemoryDataContext.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                updateOperation = false;
            }

            return updateOperation;
        }

        public void LogWriting(Command_EventParameter command_EventParameter)
        {
            string logMessage;

            switch (command_EventParameter.CommandType)
            {
                case CommandType.ExecuteHoming:
                    {
                        logMessage = "Vertical Homing";
                        break;
                    }
                default:
                    {
                        logMessage = "Unknown Action";

                        break;
                    }
            }

            this.inMemoryDataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
            this.inMemoryDataContext.SaveChanges();
        }

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

        #endregion
    }
}
