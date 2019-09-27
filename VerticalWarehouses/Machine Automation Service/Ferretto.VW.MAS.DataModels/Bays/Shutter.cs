using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Shutter : DataModel
    {
        #region Properties

        public Inverter Inverter { get; set; }

        public int TotalCycles { get; set; }

        public ShutterType Type { get; set; }

        #endregion
    }
}
