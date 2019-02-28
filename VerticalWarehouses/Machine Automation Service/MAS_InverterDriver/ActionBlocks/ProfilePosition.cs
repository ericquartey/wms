using System;

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

        public int Acceleration { get; }

        public int Deceleration { get; }

        public int Speed { get; }

        public int Quote { get; set; }

        #endregion
    }
}
