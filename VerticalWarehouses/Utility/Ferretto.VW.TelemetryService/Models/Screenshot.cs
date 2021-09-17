using System;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class ScreenShot : DataModel, IScreenShot
    {
        #region Properties

        public int BayNumber { get; set; }

        public byte[]? Image { get; set; }

        public Machine? Machine { get; set; }

        public int MachineId { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string? ViewName { get; set; }

        #endregion
    }
}
