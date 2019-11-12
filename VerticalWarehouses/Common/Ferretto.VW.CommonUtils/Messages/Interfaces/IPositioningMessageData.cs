using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface IPositioningMessageData : IMessageData
    {
        #region Properties

        Axis AxisMovement { get; }

        BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        bool ComputeElongation { get; set; }

        int Delay { get; set; }

        HorizontalMovementDirection Direction { get; set; }

        int ExecutedCycles { get; set; }

        double FeedRate { get; set; }

        bool IsOneKMachine { get; set; }

        bool IsStartedOnBoard { get; set; }

        double? LoadedNetWeight { get; }

        int? LoadingUnitId { get; }

        double LowerBound { get; }

        MovementMode MovementMode { get; set; }

        MovementType MovementType { get; }

        int RequiredCycles { get; }

        double[] SwitchPosition { get; set; }

        double[] TargetAcceleration { get; }

        int? TargetBayPositionId { get; set; }

        int? TargetCellId { get; set; }

        double[] TargetDeceleration { get; }

        double TargetPosition { get; }

        double[] TargetSpeed { get; }

        DataSample TorqueCurrentSample { get; set; }

        double UpperBound { get; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
