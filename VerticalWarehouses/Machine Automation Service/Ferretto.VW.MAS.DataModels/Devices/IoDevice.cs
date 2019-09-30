namespace Ferretto.VW.MAS.DataModels
{
    public sealed class IoDevice : DataModel
    {
        #region Properties

        public IoIndex Index { get; set; }

        public System.Net.IPAddress IpAddress { get; set; }

        public int TcpPort { get; set; }

        #endregion
    }
}
