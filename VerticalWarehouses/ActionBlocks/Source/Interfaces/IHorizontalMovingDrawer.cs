using System.Collections.Generic;

namespace Ferretto.VW.ActionBlocks
{
    public interface IHorizontalMovingDrawer
    {
        #region Properties

        InverterDriver.InverterDriver SetInverterDriverInterface { set; }

        #endregion Properties

        #region Methods

        void Initialize();

        void Run(int target, int speed, int direction, List<ProfilePosition> profile);

        void Terminate();

        #endregion Methods
    }
}
