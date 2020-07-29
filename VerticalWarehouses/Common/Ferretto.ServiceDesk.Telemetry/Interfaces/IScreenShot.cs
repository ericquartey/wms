using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IScreenShot
    {
        #region Properties

        int BayNumber { get; set; }

        int Id { get; set; }

        byte[] Image { get; set; }

        DateTimeOffset TimeStamp { get; set; }

        string ViewName { get; set; }

        #endregion
    }
}
