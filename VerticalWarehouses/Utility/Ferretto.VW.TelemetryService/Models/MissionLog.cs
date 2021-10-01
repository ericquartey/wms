using System;
using Ferretto.ServiceDesk.Telemetry;

namespace Ferretto.VW.TelemetryService.Data
{
    public class MissionLog : DataModel, IMissionLog
    {
        #region Properties

        public int Bay { get; set; }

        public int? CellId { get; set; }

        public DateTimeOffset CreationDate { get; set; }

        public string? Destination { get; set; }

        public int Direction { get; set; }

        public int EjectLoadUnit { get; set; }

        public int? LoadUnitHeight { get; set; }

        public int LoadUnitId { get; set; }

        public Machine? Machine { get; set; }

        public int MachineId { get; set; }

        public int MissionId { get; set; }

        public string? MissionType { get; set; }

        public int? NetWeight { get; set; }

        public int Priority { get; set; }

        public string Stage { get; set; } = string.Empty!;

        public string? Status { get; set; }

        public int Step { get; set; }

        public int StopReason { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
