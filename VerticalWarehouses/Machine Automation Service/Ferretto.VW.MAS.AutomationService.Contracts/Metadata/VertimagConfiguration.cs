using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(VertimagConfiguration.Metadata))]
    public partial class VertimagConfiguration
    {
        class Metadata
        {
            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<LoadingUnit> LoadingUnits { get; set; }

            public Machine Machine { get; set; }

            [ScaffoldColumn(false)]
            public SetupProceduresSet SetupProcedures { get; set; }

        }

    }
}
