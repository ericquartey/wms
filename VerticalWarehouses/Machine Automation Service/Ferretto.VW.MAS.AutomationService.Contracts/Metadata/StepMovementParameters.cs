﻿using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(StepMovementParameters.Metadata))]
    public partial class StepMovementParameters
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(5)]
            [Unit("mm/s²")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Acceleration))]
            public double Acceleration { get; set; }

            [Id(6)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.StepMovementParameters_AdjustAccelerationByWeight))]
            public bool AdjustAccelerationByWeight { get; set; }

            [Id(4)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.StepMovementParameters_AdjustSpeedByWeight))]
            public bool AdjustSpeedByWeight { get; set; }

            [Id(1)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.StepMovementParameters_Number))]
            public int Number { get; set; }

            [Id(2)]
            [Unit("mm")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.StepMovementParameters_Position))]
            public double Position { get; set; }

            [Id(3)]
            [Unit("mm/s")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Speed))]
            public double Speed { get; set; }

            #endregion
        }

        #endregion
    }
}
