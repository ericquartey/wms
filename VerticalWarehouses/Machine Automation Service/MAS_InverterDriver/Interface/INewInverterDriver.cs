using System;
using System.Collections.Generic;
using Ferretto.VW.MAS_InverterDriver.ActionBlocks;

namespace Ferretto.VW.MAS_InverterDriver
{
    public interface INewInverterDriver
    {
        #region Properties

        Single GetDrawerWeight { get; }

        #endregion

        #region Methods

        void Destroy();

        void ExecuteDrawerWeight(Int32 targetPosition, Single vMax, Single acc, Single dec);

        void ExecuteHomingStop();

        void ExecuteHorizontalHoming();

        void ExecuteHorizontalPosition(Int32 target, Int32 speed, Int32 direction, List<ProfilePosition> profile);

        void ExecuteVerticalHoming();

        void ExecuteVerticalPosition(Int32 targetPosition, Single vMax, Single acc, Single dec, Single weight,
            Int16 offset, Boolean absoluteMovement);

        void ExecuteVerticalPositionStop();

        Boolean[] GetSensorsStates();

        #endregion
    }
}
