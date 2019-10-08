using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.MoveDrawer.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.DeviceManager.MoveDrawer.Models
{
    internal class MoveDrawerMachineData : IMoveDrawerMachineData
    {
        #region Constructors

        public MoveDrawerMachineData(
            bool isOneKMachine,
            ISetupStatusProvider setupStatusProvider,
            IMachineResourcesProvider machineResourcesProvider,
            IDrawerOperationMessageData drawerOperationData,
            BayNumber requestingBay,
            IEventAggregator eventAggregator,
            ILogger<DeviceManager> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.IsOneKMachine = isOneKMachine;
            this.SetupStatusProvider = setupStatusProvider;
            this.MachineSensorsStatus = machineResourcesProvider;
            this.DrawerOperationData = drawerOperationData;
            this.RequestingBay = requestingBay;
            this.EventAggregator = eventAggregator;
            this.Logger = logger;
            this.ServiceScopeFactory = serviceScopeFactory;
        }

        #endregion

        #region Properties

        public IDrawerOperationMessageData DrawerOperationData { get; }

        public IEventAggregator EventAggregator { get; }

        public bool IsOneKMachine { get; }

        public ILogger<DeviceManager> Logger { get; }

        public IMachineResourcesProvider MachineSensorsStatus { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public ISetupStatusProvider SetupStatusProvider { get; }

        public BayNumber TargetBay { get; }

        #endregion
    }
}
