using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;

namespace Ferretto.VW.CommonUtils.Messages.Data
{
    public class SocketLinkLaserPointerChangeMessageData : ISocketLinkLaserPointerChangeMessageData
    {
        #region Constructors

        public SocketLinkLaserPointerChangeMessageData()
        {
        }

        public SocketLinkLaserPointerChangeMessageData(int commandCode, int x, int y, int z, MessageVerbosity verbosity = MessageVerbosity.Debug)
        {
            this.CommandCode = commandCode;
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.Verbosity = verbosity;
        }

        #endregion

        #region Properties

        public int CommandCode { get; set; }

        public MessageVerbosity Verbosity { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        #endregion
    }
}
