namespace Ferretto.VW.MAS.DataModels
{
    public class PositioningProcedure : SetupProcedure
    {
        #region Fields

        private double step;

        #endregion

        #region Properties

        public double Step
        {
            get => this.step;
            set
            {
                //if (value <= 0)
                //{
                //    throw new System.ArgumentOutOfRangeException(nameof(value));
                //}

                this.step = value;
            }
        }

        #endregion
    }
}
