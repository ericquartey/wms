using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer
    {
        #region Fields

        private const string ConnectionStringName = "AutomationService";

        private readonly DataLayerContext inMemoryDataContext;

        #endregion

        #region Constructors

        public DataLayer(IConfiguration configuration, DataLayerContext inMemoryDataContext)
        {
            try
            {
                this.inMemoryDataContext = inMemoryDataContext;

                this.Configuration = configuration;

                var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);

                var initialContext = new DataLayerContext(
                    new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options);

                //initialContext.Database.EnsureCreated();

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

        #endregion

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion
    }
}
