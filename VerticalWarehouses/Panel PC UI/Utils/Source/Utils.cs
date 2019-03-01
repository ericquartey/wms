namespace Ferretto.VW.Utils
{
    /// <summary>
    /// Base interface.
    /// IDriverBase interface is inherited by all device interface.
    /// </summary>
    public interface IDriverBase
    {
        #region Methods

        //! Initialize the device resources.
        bool Initialize();

        //! Terminate and release the device resources.
        void Terminate();

        #endregion
    } // interface IDriverBase

    // Creo una Queue che salva i comandi ricevuti come bytes
    //public Queue<byte[]> Commands;

    /// <summary>
    /// Data socket object.
    /// Object reference used in all asynchronous operation callbacks.
    /// </summary>
    public class SocketPacket
    {
        #region Fields

        public byte[] dataBuffer = new byte[1024];

        public System.Net.Sockets.Socket thisSocket;

        #endregion

        //|< Current socket instance
        //!< Data buffer
    }

    public class Utils
    { }
}
