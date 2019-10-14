namespace Ferretto.VW.MAS.DataModels
{
    public class VerticalResolutionCalibrationProcedure : SetupProcedure
    {
        #region Fields

        private double finalPosition;

        private double initialPosition;

        #endregion

        #region Properties

        public double FinalPosition
        {
            get => this.finalPosition;
            set
            {
                if (value <= 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.finalPosition = value;
            }
        }

        public double InitialPosition
        {
            get => this.initialPosition;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }

                this.initialPosition = value;
            }
        }

        #endregion
    }
}
