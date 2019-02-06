namespace Ferretto.VW.MAS_DataLayer
{
    public class WriteLogService : IWriteLogService
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion Fields

        #region Constructors

        public WriteLogService(DataLayerContext dataContext)
        {
            this.dataContext = dataContext;
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
