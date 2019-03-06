using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public interface IMovementMessageData : IMessageData
    {
        #region Properties

        Axis Axis { get; set; }

        decimal Displacement { get; set; }

        MovementType MovementType { get; set; }

        uint SpeedPercentage { get; set; }

        #endregion
    }
}
