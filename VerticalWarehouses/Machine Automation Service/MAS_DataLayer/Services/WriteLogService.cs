using Ferretto.Common.Common_Utils;
using Microsoft.EntityFrameworkCore;
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

            // Event Aggregator managment
            WebAPI_ExecuteActionEvent webApiExecuteActionEvent = eventAggregator.GetEvent<WebAPI_ExecuteActionEvent>();
            webApiExecuteActionEvent.Subscribe(LogWriting, ThreadOption.PublisherThread, false, logMessage => logMessage == WebAPI_Action.VerticalHoming);
        }

        #endregion Constructors

        #region Methods

        //public void LogWriting(string logMessage)
        //{
        //    this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
        //    this.dataContext.SaveChanges();
        //}

        public bool LogWriting(string logMessage)
        {
            bool updateOperation = true;

            try
            { 
                this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
                this.dataContext.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                updateOperation = false;
            }

            return updateOperation;
        }

        public void LogWriting(WebAPI_Action webApiAction)
        {
            string logMessage;

            switch (webApiAction)
            {
                case WebAPI_Action.VerticalHoming:
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
