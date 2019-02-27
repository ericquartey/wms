using System.Linq;
using Ferretto.VW.Common_Utils;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Microsoft.EntityFrameworkCore;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : IDataLayer, IWriteLogService
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private readonly DataLayerContext inMemoryDataContext;

        #endregion

        #region Constructors

        public DataLayer(string connectionString, DataLayerContext inMemoryDataContext,
            IEventAggregator eventAggregator)
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
            var webApiCommandEvent = eventAggregator.GetEvent<CommandEvent>();

            webApiCommandEvent.Subscribe(this.LogWriting);
        }

        #endregion

        #region Methods

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

        public void LogWriting(CommandMessage command_EventParameter)
        {
            string logMessage;

            switch (command_EventParameter.Type)
            {
                case MessageType.Homing:
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

        #endregion
    }
}
