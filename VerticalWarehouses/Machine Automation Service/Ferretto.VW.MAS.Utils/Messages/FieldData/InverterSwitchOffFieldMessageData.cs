using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;


namespace Ferretto.VW.MAS.Utils.Messages.FieldData
{
    public sealed class InverterSwitchOffFieldMessageData : FieldMessageData, IInverterSwitchOffFieldMessageData
    {
        #region Constructors

        public InverterSwitchOffFieldMessageData(MessageVerbosity verbosity = MessageVerbosity.Debug)
            : base(verbosity)
        {
        }

        #endregion

        #region Properties

        public FieldCommandMessage NextCommandMessage { get; set; }

        #endregion
    }
}
