using System.ComponentModel.DataAnnotations;
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

            [Id(1)]
            public double Acceleration { get; set; }

            [Id(2)]
            public bool AdjustByWeight { get; set; }

            [ScaffoldColumn(false)]
            public double Deceleration { get; set; }

            [Id(3)]
            public int Number { get; set; }

            [Id(4)]
            public double Position { get; set; }

            [Id(5)]
            public double Speed { get; set; }

            #endregion
        }

        #endregion
    }
}
