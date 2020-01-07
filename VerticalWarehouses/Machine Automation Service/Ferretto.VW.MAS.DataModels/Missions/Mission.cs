using System;
using System.Text;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ferretto.VW.MAS.DataModels
{
    public class Mission : DataModel
    {
        #region Properties

        [JsonConverter(typeof(StringEnumConverter))]
        public BayNumber CloseShutterBayNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ShutterPosition OpenShutterPosition { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MissionDeviceNotifications DeviceNotifications { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CommandAction Action { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MissionBayNotifications BayNotifications { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MissionErrorMovements ErrorMovements { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HorizontalMovementDirection Direction { get; set; }

        public bool EjectLoadUnit { get; set; }

        public DateTime CreationDate { get; set; }

        public int? DestinationCellId { get; set; }

        public Guid FsmId { get; set; }

        public string FsmRestoreStateName { get; set; }

        public string FsmStateName { get; set; }

        public int? LoadingUnitCellSourceId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LoadingUnitLocation LoadingUnitDestination { get; set; }

        public int LoadingUnitId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public LoadingUnitLocation LoadingUnitSource { get; set; }

        public MissionType MissionType { get; set; }

        public Axis NeedHomingAxis { get; set; }

        public bool NeedMovingBackward { get; set; }

        public int Priority { get; set; }

        public bool RestoreConditions { get; set; }

        public MissionStatus Status { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
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
