using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class BayPosition : DataModel
    {
        #region Properties

        [JsonIgnore]
        public Elevator Elevator { get; set; }

        public double Height { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public LoadingUnitLocation Location { get; set; }

        #endregion
    }
}
