using Ferretto.VW.MAS_InverterDriver.ActionBlocks;
using System.Collections.Generic;

namespace Ferretto.VW.MAS_InverterDriver
{
    public interface INewInverterDriver
    {
        #region Methods

        void Destroy();

        void ExecuteHorizontalHoming();

        void ExecuteHorizontalPosition(int target, int speed, int direction, List<ProfilePosition> profile);

        void ExecuteVerticalHoming();

        void ExecuteVerticalPosition(int targetPosition, float vMax, float acc, float dec, float weight, short offset);

        void ExecuteDrawerWeight(int targetPosition, float vMax, float acc, float dec);

        float GetDrawerWeight { get; }

        bool[] GetSensorsStates();

        #endregion Methods
    }
}
