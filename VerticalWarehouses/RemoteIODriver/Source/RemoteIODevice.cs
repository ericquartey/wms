using NLog;

namespace Ferretto.VW.RemoteIODriver
{
    public class RemoteIODevice : IRemoteIODriver
    {
        #region Fields

        // Logger
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #endregion Fields

        #region Properties

        public byte[] InputStateLines
        {
            get;
        }

        public string IPAddressToConnect
        {
            get;
            set;
        }

        public int PortAddressToConnect
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        public bool Initialize()
        {
            // TODO: Add your implementation code here
            return true;
        }

        public bool ReadAllInputLines()
        {
            // TODO: Add your implementation code here
            return true;
        }

        public void Terminate()
        {
            // TODO: Add your implementation code here
        }

        public bool WriteAllOutputLines(byte[] value)
        {
            // TODO: Add your implementation code here
            return true;
        }

        #endregion Methods
    }
}
