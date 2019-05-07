using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterPowerOnFieldMessageData : IInverterPowerOnFieldMessageData
    {
        #region Constructors

        public InverterPowerOnFieldMessageData(InverterIndex inverterToPowerOn, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterToPowerOn = inverterToPowerOn;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToPowerOn { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
