using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class BayPosition : DataModel
    {
        #region Properties

        public double Height { get; set; }

        public LoadingUnit LoadingUnit { get; set; }

        public LoadingUnitLocation Location { get; set; }

        #endregion
    }
}
