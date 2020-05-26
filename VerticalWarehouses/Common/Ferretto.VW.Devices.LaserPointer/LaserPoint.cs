﻿namespace Ferretto.VW.Devices.LaserPointer
{
    public class LaserPoint
    {
        #region Properties

        public int Speed { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"X={this.X}, Y={this.Y}, Z={this.Z}, V={this.Speed}";
        }

        #endregion
    }
}
