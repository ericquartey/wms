using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS_Utils.Messages.FieldData
{
    public class InverterPowerOffFieldMessageData : IInverterPowerOffFieldMessageData
    {
        #region Constructors

        public InverterPowerOffFieldMessageData(InverterIndex inverterToPowerOff, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterToPowerOff = inverterToPowerOff;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToPowerOff { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
