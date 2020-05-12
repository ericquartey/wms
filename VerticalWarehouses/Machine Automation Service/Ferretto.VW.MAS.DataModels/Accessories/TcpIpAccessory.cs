namespace Ferretto.VW.MAS.DataModels
{
    public class TcpIpAccessory : Accessory
    {
        #region Properties

        public System.Net.IPAddress IpAddress { get; set; }

        public int TcpPort { get; set; }

        #endregion
    }
}
