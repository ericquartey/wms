using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(WeightData.Metadata))]
    public partial class WeightData
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(0)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Current))]
            public double Current { get; set; }

            [Id(1)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.LUTare))]
            public double LUTare { get; set; }

            [Id(2)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.NetWeight))]
            public double NetWeight { get; set; }

            [Id(3)]
            [ScaffoldColumn(false)]
            public WeightCalibartionStep Step { get; set; }

            #endregion
        }

        #endregion
    }
}
