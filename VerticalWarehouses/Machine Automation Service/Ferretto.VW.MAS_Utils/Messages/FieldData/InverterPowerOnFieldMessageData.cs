using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterPowerOnFieldMessageData : IInverterPowerOnFieldMessageData
    {
        #region Constructors

        public InverterPowerOnFieldMessageData(
            InverterIndex inverterToPowerOn,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.InverterToPowerOn = inverterToPowerOn;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToPowerOn { get; set; }

        public FieldCommandMessage NextCommandMessage { get; set; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
