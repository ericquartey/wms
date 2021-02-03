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

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_FeedRate))]
            [Id(1)]
            public double FeedRate { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_HighSpeedDurationClose))]
            [Id(5)]
            [Unit("s/10")]
            public double HighSpeedDurationClose { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_HighSpeedDurationOpen))]
            [Id(4)]
            [Unit("s/10")]
            public double HighSpeedDurationOpen { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_HighSpeedHalfDurationClose))]
            [Id(6)]
            [Unit("s/10")]
            public double? HighSpeedHalfDurationClose { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ShutterManualParameters_HighSpeedHalfDurationOpen))]
            [Id(7)]
            [Unit("s/10")]
            public double? HighSpeedHalfDurationOpen { get; set; }

            [ScaffoldColumn(false)]
            public int Id { get; set; }

            #endregion
        }

        #endregion
    }
}
