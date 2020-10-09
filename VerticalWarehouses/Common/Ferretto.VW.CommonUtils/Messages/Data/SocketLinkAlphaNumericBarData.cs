using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class SocketLinkAlphaNumericBarData : ISocketLinkAlphaNumericBarData
    {
        #region Constructors

        public SocketLinkAlphaNumericBarData(int commandCode, int x, string textMessage, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.CommandCode = commandCode;
            this.X = x;
            this.TextMessage = textMessage;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int CommandCode { get; }

        public string TextMessage { get; }

        public MessageVerbosity Verbosity { get; }

        public int X { get; }

        #endregion
    }
}
