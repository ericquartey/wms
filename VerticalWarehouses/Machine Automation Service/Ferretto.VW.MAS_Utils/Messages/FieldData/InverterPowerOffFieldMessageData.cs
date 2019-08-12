using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public class InverterPowerOffFieldMessageData : FieldMessageData, IInverterPowerOffFieldMessageData
    {
        #region Constructors

        public InverterPowerOffFieldMessageData(
            InverterIndex inverterToPowerOff,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
            this.InverterToPowerOff = inverterToPowerOff;
        }

        #endregion

        #region Properties

        public InverterIndex InverterToPowerOff { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"InverterToPowerOff:{this.InverterToPowerOff.ToString()}";
        }

        #endregion
    }
}
