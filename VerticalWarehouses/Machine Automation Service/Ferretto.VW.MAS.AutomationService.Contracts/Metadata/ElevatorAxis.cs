using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ElevatorAxis.Metadata))]
    partial class ElevatorAxis
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(20)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.AssistedMovements))]
            public ElevatorAxisManualParameters AssistedMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_BrakeActivatePercent))]
            [Range(0D, 1D, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            [Id(1)]
            public double BrakeActivatePercent { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_BrakeReleaseTime))]
            [Unit("ms")]
            [Id(2)]
            public double BrakeReleaseTime { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_ChainOffset))]
            [Unit("mm")]
            [Id(3)]
            public double ChainOffset { get; set; }

            [Id(30)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.EmptyLoadMovement))]
            public MovementParameters EmptyLoadMovement { get; set; }

            [Id(35)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.FullLoadMovement))]
            public MovementParameters FullLoadMovement { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingCreepSpeed))]
            [Unit("mm/s")]
            [Id(4)]
            public double HomingCreepSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingFastSpeed))]
            [Unit("mm/s")]
            [Id(5)]
            public double HomingFastSpeed { get; set; }

            [Id(10)]
            [Unfold]
            public Inverter Inverter { get; set; }

            [ScaffoldColumn(false)]
            [Id(6)]
            public double LastIdealPosition { get; set; }

            [ScaffoldColumn(false)]
            [Id(7)]
            public double LowerBound { get; set; }

            [Id(25)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.ManualMovements))]
            public ElevatorAxisManualParameters ManualMovements { get; set; }

            [ScaffoldColumn(false)]
            public double Offset { get; set; }

            [ScaffoldColumn(false)]
            public Orientation Orientation { get; set; }

            [ScaffoldColumn(false)]
            public double ProfileCalibrateLength { get; set; }

            [ScaffoldColumn(false)]
            public int ProfileCalibratePosition { get; set; }

            [ScaffoldColumn(false)]
            public double ProfileCalibrateSpeed { get; set; }

            [Category("Profilo {0}")]
            [CategoryParameter(nameof(MovementProfile.Name), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(100)]
            [Offset(10)]
            public System.Collections.Generic.IEnumerable<MovementProfile> Profiles { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_Resolution))]
            [Id(8)]
            public double Resolution { get; set; }

            [ScaffoldColumn(false)]
            public int TotalCycles { get; set; }

            [ScaffoldColumn(false)]
            public double UpperBound { get; set; }

            [Id(15)]
            [Unfold]
            public WeightMeasurement WeightMeasurement { get; set; }

            #endregion
        }

        #endregion
    }
}
