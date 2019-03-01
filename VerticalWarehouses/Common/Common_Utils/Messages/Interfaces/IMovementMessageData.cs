using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IMovementMessageData : IMessageData
    {
        #region Properties

        MovementDirections Axis { get; set; }

        decimal Mm { get; set; }

        MovementType MovementType { get; set; }

        uint SpeedPercentage { get; set; }

        #endregion
    }
}
