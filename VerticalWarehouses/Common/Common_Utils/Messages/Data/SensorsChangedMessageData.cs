using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class SensorsChangedMessageData : ISensorsChangedMessageData
    {
        #region Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Performance",
            "CA1819:Properties should not return arrays",
            Justification = "Review the code to see if it is really necessary to return a plain array.")]
        public bool[] SensorsStates { get; set; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
