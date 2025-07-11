﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(WeightMeasurement.Metadata))]
    public partial class WeightMeasurement
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Id(1)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.MeasureConst0))]
            public double MeasureConst0 { get; set; }

            [Id(2)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.MeasureConst1))]
            public double MeasureConst1 { get; set; }

            [Id(3)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.MeasureConst2))]
            public double MeasureConst2 { get; set; }

            [Id(4)]
            [Unit("mm/s")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.MeasureSpeed))]
            public double MeasureSpeed { get; set; }

            [Id(0)]
            [Unit("s/10")]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.MeasureTime))]
            public int MeasureTime { get; set; }

            [Category(Category = nameof(Vertimag.WeightData), ResourceType = typeof(Vertimag))]
            [CategoryParameter(nameof(WeightData.Step), ValueStringifierType = typeof(EnumValueStringifier))]
            [Id(5)]
            [Offset(5)]
            public IEnumerable<WeightData> WeightDatas { get; set; }

            #endregion
        }

        #endregion
    }
}
