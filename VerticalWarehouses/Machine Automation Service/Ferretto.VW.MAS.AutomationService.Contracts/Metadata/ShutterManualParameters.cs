using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ShutterManualParameters.Metadata))]
    public partial class ShutterManualParameters
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [ScaffoldColumn(false)]
            public int Id { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_FeedRate))]
            [Id(1)]
            public double FeedRate { get; set; }

            [ScaffoldColumn(false)]
            public double HighSpeedDurationClose { get; set; }

            [ScaffoldColumn(false)]
            public double HighSpeedDurationOpen { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_MaxSpeed))]
            [Id(3)]
            public double MaxSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_MinSpeed))]
            [Id(2)]
            public double MinSpeed { get; set; }

            #endregion
        }

        #endregion
    }
}
