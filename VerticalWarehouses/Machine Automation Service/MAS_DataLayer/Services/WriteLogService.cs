using Ferretto.Common.Common_Utils;
using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public class WriteLogService : IWriteLogService
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion Fields

        #region Constructors

        public WriteLogService(DataLayerContext dataContext, IEventAggregator eventAggregator)
        {
            this.dataContext = dataContext;

            var webApiCommandEvent = eventAggregator.GetEvent<WebAPI_CommandEvent>();
            webApiCommandEvent.Subscribe(this.LogWriting);
        }

        #endregion Constructors

        #region Methods

        public void LogWriting(string message)
        {
            this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = message });
            this.dataContext.SaveChanges();
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

        #endregion Methods
    }
}
