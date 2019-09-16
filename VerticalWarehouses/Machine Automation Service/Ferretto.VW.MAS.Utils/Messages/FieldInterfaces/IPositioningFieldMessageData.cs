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

        decimal? LoadedNetWeight { get; set; }

        int? LoadingUnitId { get; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        decimal[] SwitchPosition { get; set; }

        decimal[] TargetAcceleration { get; set; }

        decimal[] TargetDeceleration { get; set; }

        decimal TargetPosition { get; set; }

        decimal[] TargetSpeed { get; set; }

        DataSample TorqueCurrentSample { get; set; }

        #endregion
    }
}
