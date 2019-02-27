using System;
using System.Collections.Generic;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public interface INewInverterDriver
    {
        #region Properties

        float GetDrawerWeight { get; }

        #endregion

        #region Methods

        void Destroy();

        void ExecuteDrawerWeight(int targetPosition, float vMax, float acc, float dec);

        void ExecuteHomingStop();

        void ExecuteHorizontalHoming();

        void ExecuteHorizontalPosition(int target, int speed, int direction, List<ProfilePosition> profile, float weight);

        void ExecuteHorizontalPositionStop();

        void ExecuteVerticalHoming();

        void ExecuteVerticalPosition(int targetPosition, float vMax, float acc, float dec, float weight,
            short offset, bool absoluteMovement);

        void ExecuteVerticalPositionStop();

        bool[] GetSensorsStates();

        #endregion
    }
}
