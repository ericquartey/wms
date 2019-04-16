using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_Utils.Enumerations;

namespace Ferretto.VW.MAS_Utils.Messages.FieldInterfaces
{
    public interface IPositioningFieldMessageData : IFieldMessageData
    {
        #region Properties

        Axis AxisMovement { get; set; }

        MovementType MovementType { get; set; }

        decimal TargetAcceleration { get; set; }

        decimal TargetDeceleration { get; set; }

        decimal TargetPosition { get; set; }

        decimal TargetSpeed { get; set; }

        #endregion
    }
}
