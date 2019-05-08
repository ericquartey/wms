using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;

namespace Ferretto.VW.Common_Utils.Messages.Data
{
    public class CurrentPositionMessageData : IMessageData
    {
        #region Constructors

        public CurrentPositionMessageData(decimal currentPosition)
        {
            this.CurrentPosition = currentPosition;
        }

        public CurrentPositionMessageData(int executedCycles, BeltBurnishingPosition beltBurnishingPosition)
        {
            this.ExecutedCycles = executedCycles;

            this.BeltBurnishingPosition = beltBurnishingPosition;
        }

        #endregion

        #region Properties

        public BeltBurnishingPosition BeltBurnishingPosition { get; set; }

        public decimal CurrentPosition { get; set; }

        public int ExecutedCycles { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
