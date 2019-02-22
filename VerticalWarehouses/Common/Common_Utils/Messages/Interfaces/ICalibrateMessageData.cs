using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Common_Utils.Messages.Interfaces
{
    public enum Axis
    {
        Horizontal,

        Vertical,

        Both
    }

    public interface ICalibrateMessageData : IEventMessageData
    {
        #region Properties

        Axis AxisToCalibrate { get; }

        #endregion
    }
}
