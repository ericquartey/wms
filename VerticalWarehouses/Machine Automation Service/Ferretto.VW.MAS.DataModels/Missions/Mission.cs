using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Mission : DataModel
    {
        #region Properties

        public CommandAction Action { get; set; }

        public MissionBayNotifications BayNotifications { get; set; }

        public BayNumber CloseShutterBayNumber { get; set; }

        public DateTime CreationDate { get; set; }

        public int? DestinationCellId { get; set; }

        public MissionDeviceNotifications DeviceNotifications { get; set; }

        public HorizontalMovementDirection Direction { get; set; }

        public bool EjectLoadUnit { get; set; }

        public MissionErrorMovements ErrorMovements { get; set; }

        public int? LoadUnitCellSourceId { get; set; }

        public LoadingUnitLocation LoadUnitDestination { get; set; }

        public int LoadUnitId { get; set; }

        public LoadingUnitLocation LoadUnitSource { get; set; }

        public MissionType MissionType { get; set; }

        public Axis NeedHomingAxis { get; set; }

        public bool NeedMovingBackward { get; set; }

        public ShutterPosition OpenShutterPosition { get; set; }

        public int Priority { get; set; }

        public bool RestoreConditions { get; set; }

        public MissionStep RestoreStep { get; set; }

        public MissionStatus Status { get; set; }

        public MissionStep Step { get; set; }

        public StopRequestReason StopReason { get; set; }

        public BayNumber TargetBay { get; set; }

        public int? WmsId { get; set; }

        #endregion

        #region Methods

        public bool IsMissionToRestore()
        {
            return this.RestoreStep != MissionStep.NotDefined;
        }

        public bool IsRestoringType()
        {
            return this.MissionType == MissionType.WMS
                || this.MissionType == MissionType.IN
                || this.MissionType == MissionType.Manual
                || this.MissionType == MissionType.OUT;
        }

        public override string ToString()
        {
            var returnString = "Mission:" +
                $"Id={this.Id}; " +
                $"LoadUnitId={this.LoadUnitId}; " +
                $"WmsId={this.WmsId}; " +
                $"TargetBay={this.TargetBay}; " +
                $"Step={this.Step}; " +
                $"Source={this.LoadUnitSource}; " +
                $"Destination={this.LoadUnitDestination}; " +
                $"CellDestinationId={this.DestinationCellId}; " +
                $"CellSourceId={this.LoadUnitCellSourceId}; " +
                $"RestoreStep={this.RestoreStep}; " +
                $"MissionType={this.MissionType}; " +
                $"NeedHomingAxis={this.NeedHomingAxis}; " +
                $"NeedMovingBackward={this.NeedMovingBackward}; " +
                $"RestoreConditions={this.RestoreConditions}; " +
                $"Status={this.Status}; " +
                $"CloseShutterBayNumber={this.CloseShutterBayNumber}; " +
                $"OpenShutterPosition={this.OpenShutterPosition}; " +
                $"DeviceNotifications={this.DeviceNotifications}; " +
                $"Action={this.Action}; " +
                $"BayNotifications={this.BayNotifications}; " +
                $"ErrorMovements={this.ErrorMovements}; " +
                $"Direction={this.Direction}; " +
                $"EjectLoadUnit={this.EjectLoadUnit}; " +
                $"Priority={this.Priority}; " +
                $"StopReason={this.StopReason}; ";
            return returnString;
        }

        #endregion
    }
}
