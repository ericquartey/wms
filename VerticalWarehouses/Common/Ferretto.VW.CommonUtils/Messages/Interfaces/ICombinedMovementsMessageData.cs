using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.CommonUtils.Messages.Interfaces
{
    public interface ICombinedMovementsMessageData : IMessageData
    {
        #region Properties

        IPositioningMessageData HorizontalPositioningMessageData { get; }

        IPositioningMessageData VerticalPositioningMessageData { get; }

        #endregion
    }
}
