using System;
using System.Text;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public class Mission : DataModel
    {
        #region Properties

        public BayNumber CloseShutterBayNumber;

        public BayNumber OpenShutterBayNumber;

        public MissionDeviceNotifications DeviceNotifications;

        public DateTime CreationDate { get; set; }

        public int? DestinationCellId { get; set; }

        public Guid FsmId { get; set; }

        public string FsmRestoreStateName { get; set; }

        public string FsmStateName { get; set; }

        public int? LoadingUnitCellSourceId { get; set; }

        public LoadingUnitLocation LoadingUnitDestination { get; set; }

        public int LoadingUnitId { get; set; }

        public LoadingUnitLocation LoadingUnitSource { get; set; }

        public MissionType MissionType { get; set; }

        public Axis NeedHomingAxis { get; set; }

        public bool NeedMovingBackward { get; set; }

        public int Priority { get; set; }

        public bool RestoreConditions { get; set; }

        public MissionStatus Status { get; set; }

        public BayNumber TargetBay { get; set; }

        public int? WmsId { get; set; }

        #endregion

        #region Methods

        public bool IsMissionToRestore()
        {
            return !string.IsNullOrEmpty(this.FsmRestoreStateName);
        }

        public bool IsRestoringType()
        {
            return this.MissionType == MissionType.WMS
                || this.MissionType == MissionType.IN
#if TEST_ERROR_STATE
                || this.MissionType == MissionType.Manual
#endif
                || this.MissionType == MissionType.OUT;
        }

        #endregion

        public override string ToString()
        {
            var returnString = new StringBuilder();

            returnString
                .Append("Mission:")
                .Append($"LoadingUnitId={this.LoadingUnitId}; ")
                .Append($"WmsId={this.WmsId}; ")
                .Append($"TargetBay={this.TargetBay}; ")
                .Append($"FsmStateName={this.FsmStateName}; ")
                .Append($"Source={this.LoadingUnitSource}; ")
                .Append($"Destination={this.LoadingUnitDestination}; ")
                .Append($"CellSourceId={this.LoadingUnitCellSourceId}; ")
                .Append($"FsmRestoreStateName={this.FsmRestoreStateName}; ")
                .Append($"MissionType={this.MissionType}; ")
                .Append($"NeedHomingAxis={this.NeedHomingAxis}; ")
                .Append($"NeedMovingBackward={this.NeedMovingBackward}; ")
                .Append($"RestoreConditions={this.RestoreConditions}; ")
                .Append($"Status={this.Status}; ")
                .Append($"Priority={this.Priority}; ");
            return returnString.ToString();
        }
    }
}
