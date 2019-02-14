using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Microsoft.EntityFrameworkCore;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public class WriteLogService : IWriteLogService
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public WriteLogService(DataLayerContext dataContext, IEventAggregator eventAggregator)
        {
            this.dataContext = dataContext;

            var webApiCommandEvent = eventAggregator.GetEvent<WebAPI_CommandEvent>();

            this.dataContext.Database.EnsureCreated();

            //webApiCommandEvent.Subscribe(this.LogWriting);
        }

        #endregion

        #region Methods

        public bool LogWriting(string logMessage)
        {
            var updateOperation = true;

            try
            {
                this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
                this.dataContext.SaveChanges();
            }
            catch (DbUpdateException exception)
            {
                updateOperation = false;

                throw exception;
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

            this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
            this.dataContext.SaveChanges();
        }

        #endregion
    }
}
