namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class ProfilePosition
    {
        #region Constructors

        public ProfilePosition(int quote, int speed, int acceleration, int deceleration)
        {
            this.Quote = quote;
            this.Speed = speed;
            this.Acceleration = acceleration;
            this.Deceleration = deceleration;
        }

        #endregion

        #region Properties

        public int Acceleration { get; private set; }
        public int Deceleration { get; private set; }
        public int Quote { get; set; }
        public int Speed { get; private set; }

        #endregion
    }
}
