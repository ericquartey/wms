using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.MAS.DataLayer
{
    public class BayChainPositionMessageData : IMessageData
    {
        #region Constructors

        public BayChainPositionMessageData(BayNumber bayNumber, double position)
        {
            this.BayNumber = bayNumber;
            this.Position = position;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; }

        public double Position { get; }

        public MessageVerbosity Verbosity => MessageVerbosity.Info;

        #endregion
    }
}
