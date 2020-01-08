using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ElevatorAxis.Metadata))]
    partial class ElevatorAxis
    {
        class Metadata {

            public ElevatorAxisManualParameters AssistedMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_BrakeActivatePercent))]
            [Range(0D, 1D, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            public double BrakeActivatePercent { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_BrakeReleaseTime))]
            [Unit("ms")]
            public double BrakeReleaseTime { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_ChainOffset))]
            [Unit("mm")]
            public double ChainOffset { get; set; }

            [Unfold]
            public MovementParameters EmptyLoadMovement { get; set; }

            [Unfold]
            public MovementParameters FullLoadMovement { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingCreepSpeed))]
            [Unit("mm/s")]
            public double HomingCreepSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingFastSpeed))]
            [Unit("mm/s")]
            public double HomingFastSpeed { get; set; }

            [Unfold]
            public Inverter Inverter { get; set; }

            [ScaffoldColumn(false)]
            public double LastIdealPosition { get; set; }

            [ScaffoldColumn(false)]
            public double LowerBound { get; set; }

            [Unfold]
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

            [ScaffoldColumn(false)]
            public System.Collections.Generic.IEnumerable<MovementProfile> Profiles { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_Resolution))]
            public decimal Resolution { get; set; }

            [ScaffoldColumn(false)]
            public int TotalCycles { get; set; }

            [ScaffoldColumn(false)]
            public double UpperBound { get; set; }

            [Unfold]
            public WeightMeasurement WeightMeasurement { get; set; }
        }
    }
}
