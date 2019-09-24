using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterPowerOnFieldMessageData : FieldMessageData, IInverterPowerOnFieldMessageData
    {
        #region Constructors

        public InverterPowerOnFieldMessageData(
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion

        #region Properties

        public FieldCommandMessage NextCommandMessage { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"InverterToPowerOn NextCommandMessage:{this.NextCommandMessage}";
        }

        #endregion
    }
}
