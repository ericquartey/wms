using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public class ScreenShot : IScreenShot
    {
        #region Properties

        public int BayNumber { get; set; }

        public int Id { get; set; }

        public byte[] Image { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public string ViewName { get; set; }

        #endregion
    }
}
