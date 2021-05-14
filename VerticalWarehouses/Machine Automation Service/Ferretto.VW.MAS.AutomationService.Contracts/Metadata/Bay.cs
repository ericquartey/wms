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
            [Offset(10)]
            public Carousel Carousel { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_ChainOffset))]
            [Unit("mm")]
            [Id(1)]
            public double ChainOffset { get; set; }

            [ScaffoldColumn(false)]
            public Mission CurrentMission { get; set; }

            [ScaffoldColumn(false)]
            public int? CurrentWmsMissionOperationId { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.CyclesToCalibrate))]
            [Id(140)]
            public int CyclesToCalibrate { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.EmptyLoadMovement))]
            [Id(90)]
            public MovementParameters EmptyLoadMovement { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.ExternalBay))]
            [CategoryDescription(ResourceType = typeof(Vertimag), Description = nameof(Vertimag.ExternalBay_Description))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(60)]
            public External External { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.FullLoadMovement))]
            [Id(95)]
            public MovementParameters FullLoadMovement { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Inventory))]
            [Id(130)]
            public bool Inventory { get; set; }

            [ScaffoldColumn(false)]
            public Inverter Inverter { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayIoDevice))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(20)]
            public IoDevice IoDevice { get; set; }

            [ScaffoldColumn(false)]
            public bool IsActive { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsAdjustByWeight))]
            [Id(33)]
            public bool IsAdjustByWeight { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsCheckIntrusion))]
            [Id(32)]
            public bool IsCheckIntrusion { get; set; }

            [ScaffoldColumn(false)]
            public bool IsDouble { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsExternal))]
            [Id(5)]
            public bool IsExternal { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsFastDepositToBay))]
            [Id(31)]
            public bool IsFastDepositToBay { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_IsTelescopic))]
            [Id(34)]
            public bool IsTelescopic { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayLaser))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(30)]
            public Laser Laser { get; set; }

            //[Editable(false)]
            //[Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.LastCalibrationCycles))]
            //[Id(150)]
            //public int LastCalibrationCycles { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.BayNumber))]
            [Id(6)]
            public BayNumber Number { get; set; }

            [ScaffoldColumn(false)]
            public BayOperation Operation { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Pick))]
            [Id(100)]
            public bool Pick { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Position))]
            [CategoryParameter(nameof(BayPosition.LocationUpDown), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(70)]
            [Offset(10)]
            public System.Collections.Generic.IEnumerable<BayPosition> Positions { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Put))]
            [Id(110)]
            public bool Put { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_Resolution))]
            [Id(7)]
            [Unit("imp/mm")]
            public double Resolution { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.BayShutter))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(40)]
            public Shutter Shutter { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Bay_Side))]
            [Id(8)]
            public WarehouseSide Side { get; set; }

            [ScaffoldColumn(false)]
            public BayStatus Status { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.View))]
            [Id(120)]
            public bool View { get; set; }

            #endregion
        }

        #endregion
    }
}
