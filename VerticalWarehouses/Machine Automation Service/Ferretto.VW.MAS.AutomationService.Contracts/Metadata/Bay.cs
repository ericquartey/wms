using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Bay.Metadata))]
    public partial class Bay
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayCarousel))]
            [CategoryDescription(ResourceType = typeof(Vertimag), Description = nameof(Vertimag.BayCarousel_Description))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            //[PullToRoot, Unfold]
            public Carousel Carousel { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_MaxGrossWeight))]
            [Unit("mm")]
            public double ChainOffset { get; set; }

            [ScaffoldColumn(false)]
            public Mission CurrentMission { get; set; }

            [ScaffoldColumn(false)]
            public int? CurrentWmsMissionOperationId { get; set; }

            [ScaffoldColumn(false)]
            public MovementParameters EmptyLoadMovement { get; set; }

            [ScaffoldColumn(false)]
            public MovementParameters FullLoadMovement { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayInverter))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            //[PullToRoot, Unfold]
            public Inverter Inverter { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayIoDevice))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            //[PullToRoot, Unfold]
            public IoDevice IoDevice { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsActive))]
            public bool IsActive { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsDouble))]
            public bool IsDouble { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsExternal))]
            public bool IsExternal { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayLaser))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            //[PullToRoot, Unfold]
            public Laser Laser { get; set; }

            [ScaffoldColumn(false)]
            public BayNumber Number { get; set; }

            [ScaffoldColumn(false)]
            public BayOperation Operation { get; set; }

            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<BayPosition> Positions { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_Resolution))]
            public double Resolution { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayShutter))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            //[PullToRoot, Unfold]
            public Shutter Shutter { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_Side))]
            public WarehouseSide Side { get; set; }

            [ScaffoldColumn(false)]
            public BayStatus Status { get; set; }

            #endregion
        }

        #endregion
    }
}
