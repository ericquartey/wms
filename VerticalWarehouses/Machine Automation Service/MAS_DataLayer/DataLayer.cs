using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public DataLayer(IConfiguration configuration, DataLayerContext inMemoryDataContext, IEventAggregator eventAggregator)
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

            this.Configuration = configuration;

            var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);

            var initialContext = new DataLayerContext(
                new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options);

            if (initialContext == null)
            {
                throw new DataLayerException(DataLayerExceptionEnum.DATALAYER_CONTEXT_EXCEPTION);
            }

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

            initialContext.Dispose();

            // The old WriteLogService
            var webApiCommandEvent = eventAggregator.GetEvent<WebAPI_CommandEvent>();

            webApiCommandEvent.Subscribe(this.LogWriting);
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        // TEMP - hypothesis: a bay corresponds to some unusable cells
        public int GetFreeBlockPosition(int drawerHeight)
        {
            var cellSpicing = this.GetIntegerConfigurationValue(ConfigurationValueEnum.cellSpicing);

            // INFO
            // Drawer height conversion to the cell number
            // Ceiling to round a double to the upper integer
            var cellNumber = (int)Math.Ceiling((decimal)drawerHeight / cellSpicing);

            var blockHeight = 0; // INFO - Cell number
            var cellEven = new Cell();
            var cellOdd = new Cell();

            var cellCounterEven = 0;
            var cellCounterOdd = 0;

            foreach (var cell in this.inMemoryDataContext.Cells.OrderBy(cell => cell.CellId))
            {
                if (cell.Side == Side.FrontEven)
                {
                    if (cell.Status == Status.Disabled || cell.Status == Status.Free)
                    {
                        // INFO It is the first Free Cell, it could be the beginning of a block
                        if (cellCounterEven == 0 && cell.Status == Status.Free)
                        {
                            cellEven = cell;
                        }

                        // INFO Add a new cell to the current block
                        cellCounterEven++;
                    }
                    // INFO - if we find a Unusable or Occupied block, we have to search for a new block
                    else
                    {
                        cellCounterEven = 0;
                    }
                }
                else // Odd
                {
                    if (cell.Status == Status.Disabled || cell.Status == Status.Free)
                    {
                        if (cellCounterOdd == 0 && cell.Status == Status.Free)
                        {
                            cellOdd = cell;
                        }

                        cellCounterOdd++;
                    }
                    else
                    {
                        cellCounterOdd = 0;
                    }
                }

                // INFO - if the block is high or higher the drawer we end to search for the block
                if (cellCounterEven >= cellNumber)
                {
                    blockHeight = cellCounterEven;
                    break;
                }

                if (cellCounterOdd >= cellNumber)
                {
                    blockHeight = cellCounterOdd;
                    break;
                }
            }

            // INFO - The method returns the lower block position
            return blockHeight * cellSpicing;
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
            var setCellList = true;
            if (listCells != null)
            {
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
                        throw new DataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
                    }
                }
            }
            else
            {
                setCellList = false;
            }
            return setCellList;
        }

        #endregion
    }
}
