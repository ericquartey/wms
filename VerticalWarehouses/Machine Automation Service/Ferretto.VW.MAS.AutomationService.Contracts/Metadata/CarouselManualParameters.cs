using System.
/* Unmerged change from project 'Ferretto.VW.MAS.AutomationService.Contracts (net471)'
Before:
using System.Text;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
After:
using System.ComponentModel.DataAnnotations;
using System.Text;
*/
ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(CarouselManualParameters.Metadata))]
    public partial class CarouselManualParameters
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(1)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.FeedRate))]
            [Range(0D, 1D, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            public double FeedRate { get; set; }

            [ScaffoldColumn(false)]
            public int Id { get; set; }

            #endregion
        }

        #endregion
    }
}
