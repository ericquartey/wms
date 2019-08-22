using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterPowerOffFieldMessageData : FieldMessageData, IInverterPowerOffFieldMessageData
    {


        #region Constructors

        public InverterPowerOffFieldMessageData(
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
            return $"InverterToPowerOff, NextCommandMessage:{this.NextCommandMessage}";
        }

        #endregion
    }
}
