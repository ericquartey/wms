using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class RepetitiveHorizontalMovementsMessageData : IRepetitiveHorizontalMovementsMessageData
    {
        #region Constructors

        public RepetitiveHorizontalMovementsMessageData()
        {
        }

        public RepetitiveHorizontalMovementsMessageData(
            int bayPositionId,
            int loadingUnitId,
            BayNumber bayNumber,
            int requiredCycles,
            int delay,
            MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.BayPositionId = bayPositionId;
            this.LoadingUnitId = loadingUnitId;
            this.BayNumber = bayNumber;
            this.Verbosity = verbosity;
            this.RequiredCycles = requiredCycles;
            this.Delay = delay;
        }

        public RepetitiveHorizontalMovementsMessageData(IRepetitiveHorizontalMovementsMessageData other)
        {
            if (other is null)
            {
                throw new System.ArgumentNullException(nameof(other));
            }

            this.BayPositionId = other.BayPositionId;
            this.LoadingUnitId = other.LoadingUnitId;
            this.BayNumber = other.BayNumber;
            this.RequiredCycles = other.RequiredCycles;
            this.Delay = other.Delay;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        public int BayPositionId { get; set; }

        public bool BypassConditions { get; set; } = false;

        public int Delay { get; set; }

        public int ExecutedCycles { get; set; }

        public bool IsTestStopped { get; set; }

        public int? LoadingUnitId { get; set; }

        public int RequiredCycles { get; set; }

        public int? SourceBayPositionId { get; set; }

        public int? TargetBayPositionId { get; set; }

        public MessageVerbosity Verbosity { get; set; } = MessageVerbosity.Debug;

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Repetitive Horizontal Movements  Bay:{this.BayNumber.ToString()}  NumberCycles:{this.RequiredCycles} Delay:{this.Delay}";
        }

        #endregion
    }
}
