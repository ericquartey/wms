using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.FiniteStateMachines.Interface;
using Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.FiniteStateMachines.MoveDrawer.Models
{
    internal class MoveDrawerMachineData : IMoveDrawerMachineData
    {
        #region Constructors

        public MoveDrawerMachineData(
            bool isOneKMachine,
            ISetupStatusProvider setupStatusProvider,
            IMachineSensorsStatus machineSensorsStatus,
            IGeneralInfoConfigurationDataLayer generalInfoDataLayer,
            IVerticalAxisDataLayer verticalAxis,
            IHorizontalAxisDataLayer horizontalAxis,
            IDrawerOperationMessageData drawerOperationData,
            BayNumber requestingBay,
            IEventAggregator eventAggregator,
            ILogger<FiniteStateMachines> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            this.IsOneKMachine = isOneKMachine;
            this.SetupStatusProvider = setupStatusProvider;
            this.MachineSensorsStatus = machineSensorsStatus;
            this.GeneralInfoDataLayer = generalInfoDataLayer;
            this.VerticalAxis = verticalAxis;
            this.HorizontalAxis = horizontalAxis;
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

        public IGeneralInfoConfigurationDataLayer GeneralInfoDataLayer { get; }

        public IHorizontalAxisDataLayer HorizontalAxis { get; }

        public bool IsOneKMachine { get; }

        public ILogger<FiniteStateMachines> Logger { get; }

        public IMachineSensorsStatus MachineSensorsStatus { get; }

        public BayNumber RequestingBay { get; }

        public IServiceScopeFactory ServiceScopeFactory { get; }

        public ISetupStatusProvider SetupStatusProvider { get; }

        public BayNumber TargetBay { get; }

        public IVerticalAxisDataLayer VerticalAxis { get; }

        #endregion
    }
}
