using System;

namespace Ferretto.VW.MAS.DataModels
{
    public class VerticalResolutionCalibrationProcedure : SetupProcedure
    {
        #region Fields

        private double finalPosition;

        private double initialPosition;

        private double startPosition;

        #endregion

        #region Properties

        public double FinalPosition
        {
            get => this.finalPosition;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
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
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.initialPosition = value;
            }
        }

        public double StartPosition
        {
            get => this.startPosition;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.startPosition = value;
            }
        }

        #endregion
    }
}
