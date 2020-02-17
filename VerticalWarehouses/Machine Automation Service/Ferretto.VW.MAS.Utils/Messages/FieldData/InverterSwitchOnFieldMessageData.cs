using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;


namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterSwitchOnFieldMessageData : FieldMessageData, IInverterSwitchOnFieldMessageData
    {
        #region Constructors

        public InverterSwitchOnFieldMessageData(Axis axisToSwitchOn, MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.AxisToSwitchOn = axisToSwitchOn;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitchOn { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToSwitchOn:{this.AxisToSwitchOn.ToString()}";
        }

        #endregion
    }
}
