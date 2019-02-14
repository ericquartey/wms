using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        #region Fields

        private const string CELL_NOT_FOUND_EXCEPTION = "Data Layer Exception - Cell Not Found";

        private const string ConnectionStringName = "AutomationService";

        private readonly DataLayerContext inMemoryDataContext;

        #endregion

        #region Constructors

        public DataLayer(IConfiguration configuration, DataLayerContext inMemoryDataContext)
        {
            this.inMemoryDataContext = inMemoryDataContext;

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
        }

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        #region Methods

        public List<Cell> GetCellList()
        {
            List<Cell> listCells = new List<Cell>();

            foreach (var cell in inMemoryDataContext.Cells)
            {
                listCells.Add(cell);
            }

            return listCells;
        }

        public bool SetCellList(List<Cell> listCells)
        {
            bool setCellList = true;

            if (listCells != null)
            {
                foreach (var cell in listCells)
                {
                    var inMemoryCellCurrentValue = inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == cell.CellId);

                    if (inMemoryCellCurrentValue != null)
                    {
                        inMemoryCellCurrentValue.Coord = cell.Coord;
                        inMemoryCellCurrentValue.Priority = cell.Priority;
                        inMemoryCellCurrentValue.Side = cell.Side;
                        inMemoryCellCurrentValue.Status = cell.Status;

                        inMemoryDataContext.SaveChanges();
                    }
                    else
                    {
                        throw new DataLayerException(CELL_NOT_FOUND_EXCEPTION);
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
