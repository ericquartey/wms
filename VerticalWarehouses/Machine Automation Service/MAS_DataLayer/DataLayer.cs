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

        // drawerHeight: Drawer Height in mm
        public List<Cell> GetCellList(int drawerHeight)
        {
            var listCellsEven = new List<Cell>();
            var listCellsOdd = new List<Cell>();

            //for (int i = 0; i < cm.Cells.Count; i += 2) //odd ID cell's index
            //{
            //    if (cm.Cells[i].Status == 0)
            //    {
            //        int tmp = GetLastUpperNotDisabledCellIndex(cm, i);
            //        var cb = new CellBlock(i + 1, tmp + 1, counter);
            //        cm.Blocks.Add(cb);
            //        counter++;
            //        i = tmp;
            //    }
            //}
            //for (int i = 1; i < cm.Cells.Count; i += 2)//even ID cell's index
            //{
            //    if (cm.Cells[index].Status == 0)
            //    {
            //        int tmp = GetLastUpperNotDisabledCellIndex(cm, i);
            //        var cb = new CellBlock(index + 1, tmp + 1, counter);
            //        cm.Blocks.Add(cb);
            //        counter++;
            //        i = tmp;
            //    }
            //}

            var cellEven = 0;
            var cellOdd = 0;

            //this.inMemoryDataContext.Cells
            //    .OrderBy(cell => cell.CellId)
            //    .Select(cell =>
            //    {
            //        return new Cell { };
            //    });

            foreach (var cell in this.inMemoryDataContext.Cells.OrderBy(cell => cell.CellId))
            {
                if ((int)cell.Status % 2 == 0) // Even
                {
                    listCellsEven.Add(cell);
                    cellEven++;
                }
                else // Odd
                {
                    listCellsOdd.Add(cell);
                    cellOdd++;
                }

                //if ()
                //{
                //}
            }

            return listCellsEven; // Anche Odd
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
                        throw new InMemoryDataLayerException(DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION);
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
