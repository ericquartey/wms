using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisMovement { get; set; }

        HorizontalMovementDirection Direction { get; set; }

        bool IsTorqueCurrentSamplingEnabled { get; }

        double? LoadedNetWeight { get; set; }

        int? LoadingUnitId { get; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        double[] SwitchPosition { get; set; }

        double[] TargetAcceleration { get; set; }

        double[] TargetDeceleration { get; set; }

        double TargetPosition { get; set; }

        double[] TargetSpeed { get; set; }

        DataSample TorqueCurrentSample { get; set; }

        #endregion
    }
}
