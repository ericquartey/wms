using System;

namespace Ferretto.ServiceDesk.Telemetry
{
    public interface IMissionLog
    {
        #region Properties

        int Bay { get; set; }

        int? CellId { get; set; }

        DateTimeOffset CreationDate { get; set; }

        int Direction { get; set; }

        int EjectLoadUnit { get; set; }

        int Id { get; set; }

        int? LoadUnitHeight { get; set; }

        int LoadUnitId { get; set; }

        int MissionId { get; set; }

        string MissionType { get; set; }

        int? NetWeight { get; set; }

        int Priority { get; set; }

        string Stage { get; set; }

        string Status { get; set; }

        int Step { get; set; }

        int StopReason { get; set; }

        DateTimeOffset TimeStamp { get; set; }

        int? WmsId { get; set; }

        #endregion
    }
}
