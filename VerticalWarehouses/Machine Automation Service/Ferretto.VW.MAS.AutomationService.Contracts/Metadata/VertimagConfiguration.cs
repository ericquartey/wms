using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(VertimagConfiguration.Metadata))]
    public partial class VertimagConfiguration
    {
        class Metadata
        {
            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<LoadingUnit> LoadingUnits { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Machine))]
            [Offset(100)]
            [PullToRoot, Unfold]
            public Machine Machine { get; set; }

            [ScaffoldColumn(false)]
            public SetupProceduresSet SetupProcedures { get; set; }

        }

    }
}
