using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class CurrentPositionMessageData : IMessageData
    {
        #region Constructors

        public CurrentPositionMessageData()
        {
        }

        public CurrentPositionMessageData(double currentPosition)
        {
            this.CurrentPosition = currentPosition;
        }

        public CurrentPositionMessageData(int executedCycles, BeltBurnishingPosition beltBurnishingPosition)
        {
            this.ExecutedCycles = executedCycles;

            this.BeltBurnishingPosition = beltBurnishingPosition;
        }

        public CurrentPositionMessageData(int executedCycles, double currentPosition, BeltBurnishingPosition beltBurnishingPosition)
        {
            this.ExecutedCycles = executedCycles;
            this.CurrentPosition = currentPosition;
            this.BeltBurnishingPosition = beltBurnishingPosition;
        }

        #endregion

        #region Properties

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public double? CurrentPosition { get; set; }

        public int ExecutedCycles { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"BeltBurnishingPosition:{this.BeltBurnishingPosition.ToString()} CurrentPosition:{this.CurrentPosition} ExecutedCycles:{this.ExecutedCycles}";
        }

        #endregion
    }
}
