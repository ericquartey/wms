using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;


namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterPowerEnableMessageData : IInverterPowerEnableMessageData
    {
        #region Constructors

        public InverterPowerEnableMessageData()
        {
        }

        public InverterPowerEnableMessageData(bool enable, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.Enable = enable;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public bool Enable { get; }

        public MessageVerbosity Verbosity { get; }

        #endregion
    }
}
