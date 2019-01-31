using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.VW.MAS_InverterDriver;

namespace Ferretto.VW.MAS_InverterDriver
{
    public class InverterDriver : IInverterDriver
    {
        #region Fields
        #endregion Fields

        #region Constructors

        public InverterDriver()
        {
            return;
        }

        #endregion Constructors

        #region Methods
        public void ExecuteVerticalHoming()
        {
            return;
        }

        public void ExecuteHorizontalHoming()
        {
            return;
        }

        public void ExecuteVerticalPosition(long target, float weight)
        {
            return;
        }

       public void ExecuteHorizontalPosition()
       {
            return;
       }

        public bool[] GetSensorsStates()
        {
            return null;
        }

        public float GetDrawerWeight()
        {
            return 0.0f;
        }

        #endregion Methods
    }
}
