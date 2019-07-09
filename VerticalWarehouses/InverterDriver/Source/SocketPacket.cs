namespace Ferretto.VW.Drivers.Inverter
{
    public class SocketPacket
    {
        #region Fields

        public byte[] dataBuffer = new byte[1024];

        public System.Net.Sockets.Socket thisSocket;

        #endregion
    }
}
