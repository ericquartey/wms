using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public class WeightingScale : TcpIpAccessory
    {
        #region Properties

        [JsonIgnore]
        public string PortName { get; set; }

        #endregion
    }
}
