using Ferretto.VW.MAS_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class SensorsChangedMessageData : ISensorsChangedMessageData
    {
        #region Properties

        public bool[] SensorsStates { get; set; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
