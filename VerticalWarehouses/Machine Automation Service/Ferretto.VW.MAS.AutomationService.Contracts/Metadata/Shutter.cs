using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Shutter.Metadata))]
    public partial class Shutter
    {
        class Metadata
        {
            [Category(ResourceType = typeof(Vertimag), Category =nameof(Vertimag.Shutter_AssistedMovements))]
            public ShutterManualParameters AssistedMovements { get; set; }
            
            public Inverter Inverter { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Shutter_ManualMovements))]
            public ShutterManualParameters ManualMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Shutter_Type))]
            public ShutterType Type { get; set; }
        }
    }
}
