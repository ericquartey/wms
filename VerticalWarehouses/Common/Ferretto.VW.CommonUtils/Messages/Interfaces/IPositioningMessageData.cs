using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        bool BypassConditions { get; set; }

        bool ComputeElongation { get; set; }

        /// <summary>
        /// seconds
        /// </summary>
        int DelayEnd { get; set; }

        /// <summary>
        /// milliSeconds
        /// </summary>
        int DelayStart { get; set; }

        HorizontalMovementDirection Direction { get; set; }

        int ExecutedCycles { get; set; }

        double FeedRate { get; set; }

        bool IsOneTonMachine { get; set; }

        bool IsPickupMission { get; set; }

        bool IsStartedOnBoard { get; set; }

        bool IsTestStopped { get; set; }

        double? LoadedNetWeight { get; }

        int? LoadingUnitId { get; }

        double LowerBound { get; }

        MovementMode MovementMode { get; set; }

        MovementType MovementType { get; }

        double[] ProfileConst { get; set; }

        int[] ProfileSamples { get; set; }

        int RequiredCycles { get; set; }

        int? SourceBayPositionId { get; set; }

        int? SourceCellId { get; set; }

        double[] SwitchPosition { get; set; }

        double[] TargetAcceleration { get; }

        int? TargetBayPositionId { get; set; }

        int? TargetCellId { get; set; }

        double[] TargetDeceleration { get; }

        double TargetPosition { get; set; }

        double[] TargetSpeed { get; }

        DataSample TorqueCurrentSample { get; set; }

        double UpperBound { get; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
