using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisMovement { get; set; }

        bool ComputeElongation { get; }

        HorizontalMovementDirection Direction { get; set; }

        double FeedRate { get; }

        bool IsHorizontalCalibrate { get; set; }

        bool IsPickupMission { get; }

        bool IsProfileCalibrate { get; }

        bool IsTorqueCurrentSamplingEnabled { get; }

        bool IsWeightMeasure { get; }

        double? LoadedNetWeight { get; set; }

        int? LoadingUnitId { get; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        BayNumber RequestingBay { get; set; }

        double[] SwitchPosition { get; set; }

        double[] TargetAcceleration { get; set; }

        double[] TargetDeceleration { get; set; }

        double TargetPosition { get; set; }

        double TargetPositionOriginal { get; set; }

        double[] TargetSpeed { get; set; }

        DataSample TorqueCurrentSample { get; set; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
