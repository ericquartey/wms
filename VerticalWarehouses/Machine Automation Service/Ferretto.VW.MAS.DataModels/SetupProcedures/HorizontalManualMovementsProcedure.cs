namespace Ferretto.VW.MAS.DataModels
{
    public sealed class HorizontalManualMovementsProcedure : SetupProcedure
    {
        #region Properties

        public double InitialTargetPosition { get; set; }

        public double RecoveryTargetPosition { get; set; }

        #endregion
    }
}
