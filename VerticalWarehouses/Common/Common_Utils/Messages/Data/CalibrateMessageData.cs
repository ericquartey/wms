using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    internal class CalibrateMessageData : ICalibrateMessageData
    {
        #region Constructors

        public CalibrateMessageData( Axis axisToCalibrate )
        {
            AxisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; private set; }

        #endregion
    }
}
