using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
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

        public FieldCommandMessage NextCommandMessage { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"InverterToPowerOff:{this.InverterToPowerOff.ToString()} NextCommandMessage:{this.NextCommandMessage}";
        }

        #endregion
    }
}
