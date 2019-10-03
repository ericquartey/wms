using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Inverter : DataModel
    {
        #region Fields

        public const int MasterIndex = 0;

        #endregion

        #region Properties

        public InverterIndex Index { get; set; }

        public System.Net.IPAddress IpAddress { get; set; }

        public int TcpPort { get; set; }

        public InverterType Type { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"Index: {this.Index}, Type: {this.Type}";
        }

        #endregion
    }
}
