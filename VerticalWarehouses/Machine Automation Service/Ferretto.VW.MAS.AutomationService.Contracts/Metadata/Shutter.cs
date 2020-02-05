using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Shutter.Metadata))]
    public partial class Shutter
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Shutter_AssistedMovements))]
            [Offset(15)]
            public ShutterManualParameters AssistedMovements { get; set; }

            [Offset(5)]
            public Inverter Inverter { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Shutter_ManualMovements))]
            [Offset(10)]
            public ShutterManualParameters ManualMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Shutter_Type))]
            [Id(1)]
            public ShutterType Type { get; set; }

            #endregion
        }

        #endregion
    }
}
