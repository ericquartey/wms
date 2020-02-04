using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
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
            public MovementProfileType Name { get; set; }

            [Category("Steps {0}")]
            [CategoryParameter(nameof(StepMovementParameters.Number))]
            public IEnumerable<StepMovementParameters> Steps { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.TotalDistance))]
            [Unit("mm/s")]
            public double TotalDistance { get; set; }

            #endregion
        }

        #endregion
    }
}
