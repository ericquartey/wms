using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class SocketLinkAlphaNumericBarChangeMessageData : ISocketLinkAlphaNumericBarChangeMessageData
    {
        #region Constructors

        public SocketLinkAlphaNumericBarChangeMessageData()
        {
        }

        public SocketLinkAlphaNumericBarChangeMessageData(int commandCode, int x, string textMessage, BayNumber bayNumber, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.CommandCode = commandCode;
            this.X = x;
            this.TextMessage = textMessage;
            this.Verbosity = verbosity;
            this.BayNumber = bayNumber;
        }

        #endregion

        #region Properties

        public BayNumber BayNumber { get; set; }

        public int CommandCode { get; set; }

        public string TextMessage { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        public int X { get; set; }

        #endregion
    }
}
