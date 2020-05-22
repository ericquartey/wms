using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.Devices.LaserPointer
{
    public class LaserSetPosition
    {
        #region Enums

        public enum Axises
        {
            X,

            Y
        }

        #endregion

        #region Properties

        public Axises Axis { get; set; }

        public int MesuredValue { get; set; }

        public int Position { get; set; }

        public int SetValue { get; set; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.Position} {this.Axis} {this.SetValue} {this.MesuredValue}";
        }

        #endregion
    }
}
