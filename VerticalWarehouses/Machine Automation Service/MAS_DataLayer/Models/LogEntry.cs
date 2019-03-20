using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class LogEntry
    {
        #region Properties

        public string Exception { get; set; }

        public string Level { get; set; }

        public int LogEntryID { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public DateTime TimeStamp { get; set; }

        #endregion
    }
}
