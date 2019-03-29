using System;

namespace Ferretto.VW.MAS_DataLayer
{
    public class LogEntry
    {
        #region Properties

        public string Data { get; set; }

        public string Description { get; set; }

        public string Destination { get; set; }

        public string ErrorLevel { get; set; }

        public string Exception { get; set; }

        public string Level { get; set; }

        public int LogEntryID { get; set; }

        public string LoggerName { get; set; }

        public string Message { get; set; }

        public string Source { get; set; }

        public string Status { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Type { get; set; }

        #endregion
    }
}
