using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ShutterManualParameters.Metadata))]
    public partial class ShutterManualParameters
    {
        private class Metadata
        {
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_FeedRate))]
            public double FeedRate { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_HighSpeedDurationClose))]
            public double HighSpeedDurationClose { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_HighSpeedDurationOpen))]
            public double HighSpeedDurationOpen { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_MaxSpeed))]
            public double MaxSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_MinSpeed))]
            public double MinSpeed { get; set; }
        }
    }
}
