using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IRelativeMovementMessageData : IMessageData
    {
        #region Properties

        decimal DesiredMovement { get; set; }

        MovementDirections MovementDirection { get; set; }

        #endregion
    }
}

public enum MovementDirections
{
    Vertical = 0,

    Horizontal = 1
}

public enum MovementType
{
    Absolute = 0,

    Relative = 1
}
