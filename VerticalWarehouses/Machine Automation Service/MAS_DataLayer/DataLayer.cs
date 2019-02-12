using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ferretto.VW.Common_Utils;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        private readonly DataLayerContext inMemoryDataContext;

        private const string ConnectionStringName = "AutomationService";

        private const string CELL_NOT_FOUND_EXCEPTION = "Data Layer Exception - Cell Not Found";

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        public DataLayer(IConfiguration configuration, DataLayerContext inMemoryDataContext)
        {
            try
            {
                this.inMemoryDataContext = inMemoryDataContext;

                this.Configuration = configuration;

                var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);

                var initialContext = new DataLayerContext(
                    new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options);

                initialContext.Database.EnsureCreated();
                initialContext.Database.Migrate();

                foreach (var configurationValue in initialContext.ConfigurationValues)
                {
                    this.inMemoryDataContext.ConfigurationValues.Add(configurationValue);
                }

                this.inMemoryDataContext.SaveChanges();

                initialContext.Dispose();
            }
            catch (DbUpdateException exDB)
            {
                throw new NotImplementedException("Data Layer Exception - Update Exception");
            }
            catch (ApplicationException exApp)
            {
                throw new NotImplementedException("Data Layer Exception - Application Exception");
            }
        }

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

            if (listCells == null)
            {
                setCellList = false;
            }
            else
            {
                try
                { 
                    foreach(var cell in listCells)
                    {
                        var inMemoryCellCurrentValue = inMemoryDataContext.Cells.FirstOrDefault(s => s.CellId == cell.CellId);

                        if (inMemoryCellCurrentValue != null)
                        {
                            inMemoryCellCurrentValue.Coord    = cell.Coord;
                            inMemoryCellCurrentValue.Priority = cell.Priority;
                            inMemoryCellCurrentValue.Side     = cell.Side;
                            inMemoryCellCurrentValue.Status   = cell.Status;

                            inMemoryDataContext.SaveChanges();
                        }
                        else
                        {
                            throw new ExceptionsUtils(CELL_NOT_FOUND_EXCEPTION);
                        }
                    }
                }
                catch (Exception ex)
                {
                    setCellList = false;
                }
            }

            return setCellList;
        }
    }
}
