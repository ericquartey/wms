using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Shutter : DataModel
    {
        #region Properties

        public ShutterManualParameters AssistedMovements { get; set; }

        public Inverter Inverter { get; set; }

        public ShutterManualParameters ManualMovements { get; set; }

        public double MaxSpeed { get; set; }

        public double MinSpeed { get; set; }

        public ShutterType Type { get; set; }

        #endregion
    }
}
