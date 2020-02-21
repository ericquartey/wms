using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(MovementParameters.Metadata))]
    partial class MovementParameters
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(1)]
            [Editable(true)]
            [Unit("mm/s²")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Acceleration))]
            public double Acceleration { get; set; }

            [Id(2)]
            [Editable(true)]
            [Unit("mm/s²")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Deceleration))]
            public double Deceleration { get; set; }

            [Id(3)]
            [Editable(true)]
            [Unit("mm/s")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Speed))]
            public double Speed { get; set; }

            #endregion
        }

        #endregion
    }
}
