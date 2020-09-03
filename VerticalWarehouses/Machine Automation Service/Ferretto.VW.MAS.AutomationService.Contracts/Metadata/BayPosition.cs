using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

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
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BayPosition_Height))]
            public double Height { get; set; }

            [Editable(false)]
            [Id(1)]
            public double Id { get; set; }

            [Id(7)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BayPosition_IsBlocked))]
            public double IsBlocked { get; set; }

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
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BayPosition_MaxDoubleHeight))]
            public double MaxDoubleHeight { get; set; }

            [Id(6)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BayPosition_MaxSingleHeight))]
            public double MaxSingleHeight { get; set; }

            [Id(4)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BayPosition_ProfileOffset))]
            [Unit("mm")]
            public double ProfileOffset { get; set; }

            #endregion
        }

        #endregion
    }
}
