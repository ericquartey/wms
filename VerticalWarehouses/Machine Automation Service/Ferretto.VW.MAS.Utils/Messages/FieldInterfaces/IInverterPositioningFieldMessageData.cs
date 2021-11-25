using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IInverterPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        double AbsorbedCurrent { get; set; }

        Axis AxisMovement { get; set; }

        bool ComputeElongation { get; set; }

        int Direction { get; set; }

        double FeedRate { get; set; }

        bool IsBayCalibrate { get; set; }

        bool IsHorizontalCalibrate { get; }

        bool IsPickupMission { get; set; }

        bool IsProfileCalibrate { get; set; }

        bool IsProfileCalibrateDone { get; set; }

        bool IsTorqueCurrentSamplingEnabled { get; set; }

        bool IsWeightMeasure { get; set; }

        bool IsWeightMeasureDone { get; set; }

        double? LoadedNetWeight { get; }

        int? LoadingUnitId { get; set; }

        double MeasuredWeight { get; set; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        bool RefreshAll { get; }

        BayNumber RequestingBay { get; set; }

        int StartPosition { get; set; }

        int[] SwitchPosition { get; set; }

        int[] TargetAcceleration { get; set; }

        int[] TargetDeceleration { get; set; }

        int TargetPosition { get; set; }

        double TargetPositionOriginal { get; set; }

        int[] TargetSpeed { get; set; }

        bool WaitContinue { get; set; }

        #endregion
    }
}
