using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(CarouselManualParameters.Metadata))]
    public partial class CarouselManualParameters
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            public double FeedRate { get; set; }

            [ScaffoldColumn(false)]
            public int Id { get; set; }

            #endregion
        }

        #endregion
    }
}
