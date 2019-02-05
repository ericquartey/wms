using Ferretto.Common.Common_Utils;
using Prism.Events;

namespace Ferretto.VW.MAS_DataLayer
{
    public class WriteLogService : IWriteLogService
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly IEventAggregator eventAggregator;

        #endregion Fields

        #region Constructors

        public WriteLogService(DataLayerContext dataContext, IEventAggregator eventAggregator)
        {
            this.dataContext = dataContext;

            // Subscription by filtering
            WebAPI_ExecuteActionEvent webApiExecuteActionEvent = eventAggregator.GetEvent<WebAPI_ExecuteActionEvent>();

            webApiExecuteActionEvent.Subscribe(LogWriting, ThreadOption.PublisherThread, false, logMessage => logMessage == "WebAPI_Action"); // Substitute the enumarable here
        }

        #endregion Constructors

        #region Methods

        public void LogWriting(string logMessage)
        {
            this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
            this.dataContext.SaveChanges();
        }

        #endregion Methods
    }
}
