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

        #endregion

        #region Properties

        public decimal CurrentPosition { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        #endregion
    }
}
