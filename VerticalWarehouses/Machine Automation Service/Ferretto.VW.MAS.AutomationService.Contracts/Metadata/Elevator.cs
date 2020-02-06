using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Elevator.Metadata))]
    partial class Elevator
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(Category = nameof(Vertimag.ElevatorAxis), ResourceType = typeof(Vertimag))]
            [CategoryParameter(nameof(ElevatorAxis.Orientation), ValueStringifierType = typeof(EnumValueStringifier))]
            [Offset(40)]
            [Id(10)]
            public System.Collections.Generic.IEnumerable<ElevatorAxis> Axes { get; set; }

            [ScaffoldColumn(false)]
            public BayPosition BayPosition { get; set; }

            [ScaffoldColumn(false)]
            public Cell Cell { get; set; }

            [ScaffoldColumn(false)]
            public LoadingUnit LoadingUnit { get; set; }

            [ScaffoldColumn(false)]
            public int? LoadingUnitId { get; set; }

            public ElevatorStructuralProperties StructuralProperties { get; set; }

            #endregion
        }

        #endregion
    }
}
