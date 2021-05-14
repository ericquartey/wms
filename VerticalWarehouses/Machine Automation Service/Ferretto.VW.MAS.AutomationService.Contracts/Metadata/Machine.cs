using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Machine.Metadata))]
    public partial class Machine
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(Category = nameof(Vertimag.Bay), ResourceType = typeof(Vertimag))]
            [CategoryParameter(nameof(Bay.Number), ValueStringifierType = typeof(EnumValueStringifier))]
            [CategoryDescription(ResourceType = typeof(Vertimag), Description = nameof(Vertimag.Bay_CategoryDescription))]
            [Id(200)]
            [Offset(100)]
            [PullToRoot, Unfold]
            public System.Collections.Generic.IEnumerable<Bay> Bays { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Box))]
            [Id(18)]
            public bool Box { get; set; }

            [Category(Category = nameof(Vertimag.Elevator), ResourceType = typeof(Vertimag))]
            [CategoryDescription(ResourceType = typeof(Vertimag), Description = nameof(Vertimag.Elevator_CategoryDescription))]
            [Offset(500)]
            [PullToRoot, Unfold]
            public Elevator Elevator { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.EnabeNoteRules))]
            [Id(22)]
            public bool EnabeNoteRules { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_ExpireCountPrecent))]
            [Id(11)]
            public double ExpireCountPrecent { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_ExpireDays))]
            [Id(12)]
            public double ExpireDays { get; set; }

            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_Height))]
            [Id(3)]
            public double Height { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_HorizontalCyclesToCalibrate))]
            [Id(9)]
            public int HorizontalCyclesToCalibrate { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_HorizontalPositionToCalibrate))]
            [Id(10)]
            public int HorizontalPositionToCalibrate { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_Id))]
            [Id(14)]
            public int Id { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_IsAxisChanged))]
            [Id(21)]
            public bool IsAxisChanged { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_IsHeartBeat))]
            [Id(20)]
            public bool IsHeartBeat { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ItemUniqueIdLength))]
            [Id(19)]
            public int ItemUniqueIdLength { get; set; }

            [Editable(true)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadDepth))]
            [Id(4)]
            public double LoadUnitDepth { get; set; }

            [Editable(true)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMaxHeight))]
            [Id(16)]
            public double LoadUnitMaxHeight { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMaxNetWeight))]
            [Unit("kg")]
            [Id(6)]
            public double LoadUnitMaxNetWeight { get; set; }

            [Editable(true)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitMinHeight))]
            [Id(5)]
            public double LoadUnitMinHeight { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitTare))]
            [Unit("kg")]
            [Id(7)]
            public double LoadUnitTare { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitVeryHeavyPercent))]
            [Unit("%")]
            [Id(15)]
            public double LoadUnitVeryHeavyPercent { get; set; }

            [Editable(true)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_LoadUnitWidth))]
            [Id(17)]
            public double LoadUnitWidth { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_MaxGrossWeight))]
            [Unit("kg")]
            [Id(8)]
            public double MaxGrossWeight { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_ModelName))]
            [Id(1)]
            public string ModelName { get; set; }

            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<CellPanel> Panels { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_SerialNumber))]
            [Id(2)]
            public string SerialNumber { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_Simulation))]
            [Id(13)]
            public bool Simulation { get; set; }

            [Editable(true)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Machine_VerticalCyclesToCalibrate))]
            [Id(23)]
            public int VerticalCyclesToCalibrate { get; set; }

            #endregion
        }

        #endregion
    }
}
