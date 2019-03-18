using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.MAS_Utils.Messages.Interfaces;

namespace Ferretto.VW.MAS_Utils.Messages.Data
{
    public class UpDownRepetitiveMessageData : IUpDownRepetitiveMessageData
    {
        #region Constructors

        public UpDownRepetitiveMessageData(decimal targetUpperBound, decimal targetLowerBound, int numberOfRequiredCycles, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.TargetUpperBound = targetUpperBound;
            this.TargetLowerBound = targetLowerBound;
            this.NumberOfRequiredCycles = numberOfRequiredCycles;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int NumberOfRequiredCycles { get; private set; }

        public decimal TargetLowerBound { get; private set; }

        public decimal TargetUpperBound { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
