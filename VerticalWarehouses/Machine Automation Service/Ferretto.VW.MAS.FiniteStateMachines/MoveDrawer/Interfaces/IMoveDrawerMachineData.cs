using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;

namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces
{
    public interface IMoveDrawerMachineData : IMachineData
    {


        #region Properties

        IDrawerOperationMessageData DrawerOperationData { get; }

        IGeneralInfoConfigurationDataLayer GeneralInfoDataLayer { get; }

        IHorizontalAxisDataLayer HorizontalAxis { get; }

        bool IsOneKMachine { get; }

        IMachineSensorsStatus MachineSensorsStatus { get; }

        ISetupStatusProvider SetupStatusProvider { get; }

        IVerticalAxisDataLayer VerticalAxis { get; }

        #endregion
    }
}
