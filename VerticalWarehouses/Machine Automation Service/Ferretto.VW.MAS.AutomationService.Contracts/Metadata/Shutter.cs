﻿using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Shutter.Metadata))]
    public partial class Shutter
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Shutter_AssistedMovements))]
            [Id(14)]
            public ShutterManualParameters AssistedMovements { get; set; }

            [ScaffoldColumn(false)]
            public Inverter Inverter { get; set; }

            [Category(ResourceType = typeof(Vertimag), Category = nameof(Vertimag.Shutter_ManualMovements))]
            [HideProperties(nameof(ShutterManualParameters.HighSpeedDurationOpen), nameof(ShutterManualParameters.HighSpeedDurationClose))]
            [Id(7)]
            public ShutterManualParameters ManualMovements { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Shutter_Type))]
            [Id(5)]
            public ShutterType Type { get; set; }

            #endregion
        }

        #endregion
    }
}
