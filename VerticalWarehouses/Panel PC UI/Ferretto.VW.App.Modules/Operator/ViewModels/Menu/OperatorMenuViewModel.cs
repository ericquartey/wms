using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class OperatorMenuViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        // private readonly int bayNumber;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly IOperatorNavigationService operatorNavigationService;

        private readonly ISessionService sessionService;

        private readonly IWmsDataProvider wmsDataProvider;

        private bool areItemsEnabled;

        private DelegateCommand drawerActivityButtonCommand;

        private DelegateCommand immediateLoadingUnitCallMenuCommand;

        private bool isWmsEnabled;

        private MachineIdentity machineIdentity;

        private DelegateCommand showItemListsCommand;

        private DelegateCommand showItemSearchCommand;

        #endregion

        #region Constructors

        public OperatorMenuViewModel(
            IBayManager bayManager,
            ISessionService sessionService,
            IOperatorNavigationService operatorNavigationService,
            IMachineBaysWebService machineBaysWebService,
            IMissionOperationsService missionOperationsService,
            IWmsDataProvider wmsDataProvider)
        : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.operatorNavigationService = operatorNavigationService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
        }

        #endregion

        #region Properties

        public bool AreItemsEnabled
        {
            get => this.areItemsEnabled;
            private set => this.SetProperty(ref this.areItemsEnabled, value);
        }

        public int BayNumber => (int)this.MachineService?.BayNumber;

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand
            ??
            (this.drawerActivityButtonCommand = new DelegateCommand(
                this.operatorNavigationService.NavigateToDrawerView,
                this.CanShowItemOperations));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ImmediateLoadingUnitCallMenuCommand =>
            this.immediateLoadingUnitCallMenuCommand
            ??
            (this.immediateLoadingUnitCallMenuCommand = new DelegateCommand(
                this.ShowLoadingUnits,
                this.CanShowOtherMenu));

        public bool IsWmsEnabled
        {
            get => this.isWmsEnabled;
            private set => this.SetProperty(ref this.isWmsEnabled, value, this.RaiseCanExecuteChanged);
        }

        public override bool KeepAlive => true;

        public MachineIdentity MachineIdentity
        {
            get => this.machineIdentity;
            set => this.SetProperty(ref this.machineIdentity, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ShowItemListsCommand =>
            this.showItemListsCommand
            ??
            (this.showItemListsCommand = new DelegateCommand(
                this.ShowItemLists,
                this.CanShowItemLists));

        public ICommand ShowItemSearchCommand =>
            this.showItemSearchCommand
            ??
            (this.showItemSearchCommand = new DelegateCommand(
                this.ShowItemSearch,
                this.CanShowItemSearch));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            this.MachineIdentity = this.sessionService.MachineIdentity;

            this.IsWmsEnabled = this.wmsDataProvider.IsEnabled;

            await base.OnAppearedAsync();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            if (e is null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            await base.OnMachinePowerChangedAsync(e);

            this.AreItemsEnabled = e.MachinePowerState is MachinePowerState.Powered;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.drawerActivityButtonCommand?.RaiseCanExecuteChanged();
            this.immediateLoadingUnitCallMenuCommand?.RaiseCanExecuteChanged();
            this.showItemListsCommand?.RaiseCanExecuteChanged();
            this.showItemSearchCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.MachineIdentity));
            this.RaisePropertyChanged(nameof(this.BayNumber));
        }

        private bool CanShowItemLists()
        {
            return this.IsWmsEnabled && this.IsWmsHealthy;
        }

        private bool CanShowItemOperations()
        {
            return !this.IsWmsEnabled || this.IsWmsHealthy;
        }

        private bool CanShowItemSearch()
        {
            return this.IsWmsEnabled && this.IsWmsHealthy;
        }

        private bool CanShowOtherMenu()
        {
            return !this.IsWmsEnabled || this.IsWmsHealthy;
        }

        private void ShowItemLists()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.WaitingLists.MAIN,
                null,
                trackCurrentView: true);
        }

        private void ShowItemSearch()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.MAIN,
                null,
                trackCurrentView: true);
        }

        private void ShowLoadingUnits()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.IMMEDIATELOADINGUNITCALL,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
