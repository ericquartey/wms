namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(StepMovementParameters.Metadata))]
    public partial class StepMovementParameters
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            public double Acceleration { get; set; }

            public bool AdjustByWeight { get; set; }

            public double Deceleration { get; set; }

            public int Number { get; set; }

            public double Position { get; set; }

            public double Speed { get; set; }

            #endregion
        }

        #endregion
    }
}
