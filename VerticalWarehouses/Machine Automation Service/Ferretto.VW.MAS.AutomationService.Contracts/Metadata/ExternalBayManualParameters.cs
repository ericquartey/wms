using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(ExternalBayManualParameters.Metadata))]
    public partial class ExternalBayManualParameters
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
