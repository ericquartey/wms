using System;
using Microsoft.EntityFrameworkCore;

namespace MAS_DataLayer
{

    public class WriteLogService : IWriteLogService
    {
        private readonly DataLayerContext dataContext;

        public WriteLogService(DataLayerContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public void LogWriting(string logMessage)
        {
            this.dataContext.StatusLogs.Add(new StatusLog { LogMessage = logMessage });
            this.dataContext.SaveChanges();
        }
    }
}
