namespace Ferretto.VW.MAS.InverterDriver
{
    public class ImplicitReceivedEventArgs
    {
        #region Fields

        public ushort emergencyError;

        public ushort emergencyManufacturerError;

        public byte emergencyNode;

        public byte emergencyRegister;

        public bool isEmergency;

        public bool isNMT;

        public bool isOk;

        public bool isSync;

        public byte nMTNode;

        public byte nMTState;

        public byte node;

        public byte[] receivedMessage;

        #endregion
    }
}
