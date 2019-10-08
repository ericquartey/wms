using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers;

namespace Ferretto.VW.MAS.DeviceManager.MoveDrawer.Interfaces
{
    internal interface IMoveDrawerMachineData : IMachineData
    {
        #region Properties

        IDrawerOperationMessageData DrawerOperationData { get; }

        bool IsOneKMachine { get; }

        IMachineResourcesProvider MachineSensorsStatus { get; }

        ISetupStatusProvider SetupStatusProvider { get; }

        #endregion
    }
}
