using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(MovementProfile.Metadata))]
    public partial class MovementProfile
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Editable(false)]
            [Id(8)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Name))]
            public MovementProfileType Name { get; set; }

            [Category("Step {0}")]
            [CategoryParameter(nameof(StepMovementParameters.Number))]
            [Offset(10)]
            public IEnumerable<StepMovementParameters> Steps { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.TotalDistance))]
            [Unit("mm")]
            [Id(9)]
            public double TotalDistance { get; set; }

            #endregion
        }

        #endregion
    }
}
