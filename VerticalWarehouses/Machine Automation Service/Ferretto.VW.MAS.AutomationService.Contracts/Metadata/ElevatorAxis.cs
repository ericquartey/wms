﻿using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ElevatorAxis.Metadata))]
    public partial class ElevatorAxis
    {
        #region Classes

        private class Metadata
        {
            //[Id(30)]
            //[Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.AssistedMovements))]
            //[HideProperties(nameof(ElevatorAxisManualParameters.TargetDistance), nameof(ElevatorAxisManualParameters.TargetDistanceAfterZero))]
            //public ElevatorAxisManualParameters AssistedMovements { get; set; }

            //[Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_BrakeActivatePercent))]
            //[Range(0D, 100D, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            //[Unit("%")]
            //[Id(1)]
            //public double BrakeActivatePercent { get; set; }

            //[Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_BrakeReleaseTime))]
            //[Unit("ms")]
            //[Id(2)]
            //public double BrakeReleaseTime { get; set; }

            #region Properties

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Center))]
            [Range(-10, 10)]
            public int Center { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_ChainOffset))]
            [Unit("mm")]
            [Id(3)]
            public double ChainOffset { get; set; }

            [Id(40)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.EmptyLoadMovement))]
            public MovementParameters EmptyLoadMovement { get; set; }

            [Id(45)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.FullLoadMovement))]
            public MovementParameters FullLoadMovement { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingAcceleration))]
            [Unit("mm/s2")]
            [Id(6)]
            public double HomingAcceleration { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingCreepSpeed))]
            [Unit("mm/s")]
            [Id(4)]
            public double HomingCreepSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HomingFastSpeed))]
            [Unit("mm/s")]
            [Id(5)]
            public double HomingFastSpeed { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_HorizontalCalibrateSpeed))]
            [Unit("mm/s")]
            [Id(16)]
            public double HorizontalCalibrateSpeed { get; set; }

            [Id(20)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Inverter))]
            public Inverter Inverter { get; set; }

            [ScaffoldColumn(false)]
            [Id(7)]
            public double LastIdealPosition { get; set; }

            [Id(8)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_LowerBound))]
            public double LowerBound { get; set; }

            [Id(35)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.ManualMovements))]
            public ElevatorAxisManualParameters ManualMovements { get; set; }

            [ScaffoldColumn(false)]
            public double Offset { get; set; }

            [ScaffoldColumn(false)]
            public Orientation Orientation { get; set; }

            //[Id(13)]
            //[Unit("mm")]
            //[Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_ProfileCalibrateLength))]
            //public double ProfileCalibrateLength { get; set; }

            //[Id(14)]
            //[Unit("mm")]
            //[Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_ProfileCalibratePosition))]
            //public int ProfileCalibratePosition { get; set; }

            //[Id(15)]
            //[Unit("mm/s")]
            //[Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_ProfileCalibrateSpeed))]
            //public double ProfileCalibrateSpeed { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.ElevatorAxis_Profiles))]
            [CategoryParameter(nameof(MovementProfile.Name), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(100)]
            [Offset(50)]
            public System.Collections.Generic.IEnumerable<MovementProfile> Profiles { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_Resolution))]
            [Id(9)]
            [Unit("imp/mm")]
            public double Resolution { get; set; }

            [Unit("mm")]
            [Id(10)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_UpperBound))]
            public double UpperBound { get; set; }

            [Unit("s/10")]
            [Range(0, 20)]
            [Id(16)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_VerticalDepositCompensationDelay))]
            public int? VerticalDepositCompensationDelay { get; set; }

            [Unit("mm")]
            [Id(11)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_VerticalDepositOffset))]
            public double? VerticalDepositOffset { get; set; }

            [Unit("s/10")]
            [Range(0, 20)]
            [Id(17)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_VerticalPickupCompensationDelay))]
            public int? VerticalPickupCompensationDelay { get; set; }

            [Unit("mm")]
            [Id(12)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.ElevatorAxis_VerticalPickupOffset))]
            public double? VerticalPickupOffset { get; set; }

            [Id(25)]
            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.ElevatorWeight))]
            [Offset(25)]
            public WeightMeasurement WeightMeasurement { get; set; }

            #endregion
        }

        #endregion
    }
}
