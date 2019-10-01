using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces
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
