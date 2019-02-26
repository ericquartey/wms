using System;

namespace Ferretto.VW.MAS_InverterDriver.ActionBlocks
{
    public class ProfilePosition
    {
        #region Constructors

        public ProfilePosition(Int32 quote, Int32 speed, Int32 acceleration, Int32 deceleration)
        {
            this.Quote = quote;
            this.Speed = speed;
            this.Acceleration = acceleration;
            this.Deceleration = deceleration;
        }

        #endregion

        #region Properties

        public Int32 Acceleration { get; }

        public Int32 Deceleration { get; }

        public Int32 Speed { get; }

        public Int32 Quote { get; set; }

        #endregion
    }
}
