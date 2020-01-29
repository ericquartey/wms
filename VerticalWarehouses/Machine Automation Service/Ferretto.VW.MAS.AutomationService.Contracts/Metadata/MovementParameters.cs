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
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Acceleration))]
            public double Acceleration { get; set; }

            [Id(2)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Deceleration))]
            public double Deceleration { get; set; }

            [Id(4)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.FeedRate))]
            public double FeedRate { get; set; }

            [Id(3)]
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Speed))]
            public double Speed { get; set; }

            #endregion
        }

        #endregion
    }
}
