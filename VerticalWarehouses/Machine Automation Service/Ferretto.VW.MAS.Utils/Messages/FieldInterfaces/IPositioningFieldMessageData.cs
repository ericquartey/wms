using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.Utils.Messages.FieldInterfaces
{
    public interface IPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisMovement { get; set; }

        int Direction { get; set; }

        decimal LoadedGrossWeight { get; set; }

        MovementType MovementType { get; set; }

        int NumberCycles { get; }

        decimal[] SwitchPosition { get; set; }

        decimal[] TargetAcceleration { get; set; }

        decimal[] TargetDeceleration { get; set; }

        decimal TargetPosition { get; set; }

        decimal[] TargetSpeed { get; set; }

        (decimal Value, System.DateTime TimeStamp) TorqueCurrentSample { get; set; }

        #endregion
    }
}
