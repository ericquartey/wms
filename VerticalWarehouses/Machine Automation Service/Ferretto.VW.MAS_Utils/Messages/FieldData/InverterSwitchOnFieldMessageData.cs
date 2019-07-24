using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterSwitchOnFieldMessageData : IInverterSwitchOnFieldMessageData
    {
        #region Constructors

        public InverterSwitchOnFieldMessageData(Axis axisToSwitchOn, InverterIndex systemIndex, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.AxisToSwitchOn = axisToSwitchOn;
            this.SystemIndex = systemIndex;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public Axis AxisToSwitchOn { get; }

        public InverterIndex SystemIndex { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"AxisToSwitchOn:{this.AxisToSwitchOn.ToString()} SystemIndex:{this.SystemIndex.ToString()}";
        }

        #endregion
    }
}
