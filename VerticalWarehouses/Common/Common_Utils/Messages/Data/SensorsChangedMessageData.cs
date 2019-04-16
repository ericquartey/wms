using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class SensorsChangedMessageData : ISensorsChangedMessageData
    {
        #region Properties

        public bool[] SensorsStates { get; set; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
