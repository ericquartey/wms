using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Ferretto.VW.MAS_DataLayer
{
    public class DataLayer : IDataLayer
    {
        private const string ConnectionStringName = "AutomationService";

        #region Properties

        public IConfiguration Configuration { get; }

        #endregion

        public DataLayer(IConfiguration configuration, DataLayerContext inMemoryDataContext)
        {
            try
            { 
                this.Configuration = configuration;

                var connectionString = this.Configuration.GetConnectionString(ConnectionStringName);

                var initialContext = new DataLayerContext(
                    new DbContextOptionsBuilder<DataLayerContext>().UseSqlite(connectionString).Options);

                // Insert here the link between the InMemory DB and the associated model
                foreach(var statusLog in initialContext.StatusLogs)
                {
                    inMemoryDataContext.Add(statusLog);
                }

                inMemoryDataContext.SaveChanges();

                // To delete the SQLite table content
                initialContext.Dispose();
            }
            catch (DbUpdateException exDB)
            {
                
            }
            catch(ApplicationException exApp)
            {
                
            }
        }
    }
}
