using System;

namespace Ferretto.VW.Telemetry.Contracts.Hub.Model
{
    public class MissionLog
    {
        #region Properties

        public int Bay { get; set; }

        public int? CellId { get; set; }

        public DateTimeOffset CreationDate { get; set; }

        public string Destination { get; set; }

        public int Direction { get; set; }

        public int EjectLoadUnit { get; set; }

        public int Id { get; set; }

        public int LoadUnitId { get; set; }

        public int MissionId { get; set; }

        public string MissionType { get; set; }

        public int Priority { get; set; }

        public string Status { get; set; }

        public int Step { get; set; }

        public int StopReason { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public int? WmsId { get; set; }

        #endregion
    }
}
