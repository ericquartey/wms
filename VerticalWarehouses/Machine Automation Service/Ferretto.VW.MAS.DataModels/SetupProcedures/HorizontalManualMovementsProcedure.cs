namespace Ferretto.VW.MAS.DataModels
{
    public sealed class HorizontalManualMovementsProcedure : SetupProcedure
    {
        #region Fields

        private double initialTargetPosition;

        private double recoveryTargetPosition;

        #endregion

        #region Properties

        public double InitialTargetPosition
        {
            get => this.initialTargetPosition;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.initialTargetPosition = value;
            }
        }

        public double RecoveryTargetPosition
        {
            get => this.recoveryTargetPosition;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.recoveryTargetPosition = value;
            }
        }

        #endregion
    }
}
