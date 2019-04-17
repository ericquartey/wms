using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.Common_Utils.Messages.Data
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

        public decimal CurrentPosition { get; set; }

        public int NumberOfCompletedCycles { get; set; }

        public int NumberOfRequiredCycles { get; private set; }

        public decimal TargetLowerBound { get; private set; }

        public decimal TargetUpperBound { get; private set; }

        public MessageVerbosity Verbosity { get; private set; }

        #endregion
    }
}
