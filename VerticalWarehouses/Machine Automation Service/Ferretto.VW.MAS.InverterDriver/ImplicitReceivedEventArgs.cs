namespace Ferretto.VW.MAS.InverterDriver
{
    public class ImplicitReceivedEventArgs
    {
        #region Fields

        public ushort EmergencyError;

        public ushort EmergencyManufacturerError;

        public byte EmergencyNode;

        public byte EmergencyRegister;

        public bool isEmergency;

        public bool isNMT;

        public bool isOk;

        public bool isSync;

        public byte node;

        public byte[] receivedMessage;

        #endregion
    }
}
