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

        public CurrentPositionMessageData(int executedCycles, BeltBreakInPosition beltBreakInPosition)
        {
            this.ExecutedCycles = executedCycles;

            this.BeltBreakInPosition = beltBreakInPosition;
        }

        #endregion

        #region Properties

        public BeltBreakInPosition BeltBreakInPosition { get; set; }

        public decimal CurrentPosition { get; set; }

        public int ExecutedCycles { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
