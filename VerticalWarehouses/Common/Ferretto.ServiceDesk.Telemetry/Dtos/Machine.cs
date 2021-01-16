namespace Ferretto.ServiceDesk.Telemetry
{
    public class Machine : IMachine
    {
        #region Properties

        public string ModelName { get; set; }

        public byte[] RawDatabaseContent { get; set; }

        public string SerialNumber { get; set; }

        public string Version { get; set; }

        #endregion
    }
}
