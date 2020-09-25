using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IIOLog
    {
        #region Properties

        int BayNumber { get; set; }

        string Description { get; set; }

        string Input { get; set; }

        string Output { get; set; }

        DateTimeOffset TimeStamp { get; set; }

        #endregion
    }
}
