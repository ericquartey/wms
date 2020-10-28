using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class InverterStopMessageData : IMessageData
    {
        #region Constructors

        public InverterStopMessageData()
        {
        }

        #endregion

        #region Properties

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
