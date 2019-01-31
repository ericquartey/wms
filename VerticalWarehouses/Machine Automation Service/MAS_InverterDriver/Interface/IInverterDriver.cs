using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS_InverterDriver
{
    public interface IInverterDriver
    {
        #region Methods

        void ExecuteVerticalHoming();
        void ExecuteHorizontalHoming();
        void ExecuteVerticalPosition(int target, float weight);
        void ExecuteHorizontalPosition();
        bool[] GetSensorsStates();
        float GetDrawerWeight();

        #endregion Methods  
    }
}   
