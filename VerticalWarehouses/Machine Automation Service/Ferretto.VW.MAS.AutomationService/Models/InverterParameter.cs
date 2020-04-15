using Ferretto.VW.MAS.InverterDriver.Contracts;

namespace Ferretto.VW.MAS.AutomationService
{
    public class InverterParameter
    {
        #region Properties

        public int DataSet { get; set; }

        public InverterParameterId Id { get; set; }

        public string Type { get; set; }

        public int Value { get; set; }

        #endregion
    }
}
