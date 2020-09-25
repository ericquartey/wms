using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public class IOLog : IIOLog
    {
        #region Properties

        public int BayNumber { get; set; }

        public string Description { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        #endregion
    }
}
