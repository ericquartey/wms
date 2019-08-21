using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class CalibrateAxisFieldMessageData : FieldMessageData, ICalibrateAxisFieldMessageData
    {
        #region Constructors

        public CalibrateAxisFieldMessageData(Axis axisToCalibrate, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisToCalibrate = axisToCalibrate;
        }

        #endregion

        #region Properties

        public Axis AxisToCalibrate { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToCalibrate:{this.AxisToCalibrate.ToString()}";
        }

        #endregion
    }
}
