using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(BayPosition.Metadata))]
    public partial class BayPosition
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(3)]
            public double Height { get; set; }

            [Editable(false)]
            [Id(1)]
            public double Id { get; set; }

            [ScaffoldColumn(false)]
            public bool IsUpper { get; set; }

            [ScaffoldColumn(false)]
            public LoadingUnit LoadingUnit { get; set; }

            [Id(2)]
            [Editable(false)]
            public LoadingUnitLocation Location { get; set; }

            [ScaffoldColumn(false)]
            public LoadingUnitLocation LocationUpDown { get; set; }

            [Id(5)]
            [Unit("mm")]
            public double MaxDoubleHeight { get; set; }

            [Id(6)]
            [Unit("mm")]
            public double MaxSingleHeight { get; set; }

            [Id(4)]
            [Unit("mm")]
            public double ProfileOffset { get; set; }

            #endregion
        }

        #endregion
    }
}
