using System;
using System.Configuration;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class OperatorMenuViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private readonly ISessionService sessionService;

        private bool areItemsEnabled;

        private int bayNumber;

        private bool checkOnlyFirstAppeared;

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
            IMachineBaysWebService machineBaysWebService,
            IMissionOperationsService missionOperationsService)
        : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));

            this.checkOnlyFirstAppeared = true;
        }

        #endregion

        #region Properties

        public bool AreItemsEnabled
        {
            get => this.areItemsEnabled;
            private set => this.SetProperty(ref this.areItemsEnabled, value);
        }

        public int BayNumber
        {
            get => (int)this.MachineService?.BayNumber;
        }

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand
            ??
            (this.drawerActivityButtonCommand = new DelegateCommand(
                () => this.ShowItemOperations(true),
                this.CanShowItemOperations));

        public override EnableMask EnableMask => EnableMask.Any;

        public ICommand ImmediateLoadingUnitCallMenuCommand =>
            this.immediateLoadingUnitCallMenuCommand
            ??
            (this.immediateLoadingUnitCallMenuCommand = new DelegateCommand(
                this.ImmediateLoadingUnitCallMenu,
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

            if (this.Data is MachineIdentity machineIdentity)
            {
                this.MachineIdentity = machineIdentity;
            }
            else
            {
                this.MachineIdentity = this.sessionService.MachineIdentity;
            }

            this.IsWmsEnabled = ConfigurationManager.AppSettings.GetWmsDataServiceEnabled();

            await base.OnAppearedAsync();
        }

        protected override async Task OnDataRefreshAsync()
        {
            await this.machineBaysWebService.ActivateAsync();

            this.CheckForNewOperationsAsync();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
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
            return
                !this.IsWaitingForResponse
                &&
                this.IsWmsEnabled;
        }

        private bool CanShowItemOperations()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanShowItemSearch()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.IsWmsEnabled;
        }

        private bool CanShowOtherMenu()
        {
            return !this.IsWaitingForResponse;
        }

        private void CheckForNewOperation()
        {
            if (this.MachineModeService.MachineMode != MachineMode.Automatic)
            {
                return;
            }

            this.ShowItemOperations(false);
        }

        private async Task CheckForNewOperationsAsync()
        {
            while (this.IsVisible &&
                   this.checkOnlyFirstAppeared)
            {
                this.CheckForNewOperation();
                await Task.Delay(1000);
            }
        }

        private void ImmediateLoadingUnitCallMenu()
        {
            this.checkOnlyFirstAppeared = false;
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.IMMEDIATELOADINGUNITCALL,
                null,
                trackCurrentView: true);
        }

        private void ShowItemLists()
        {
            this.checkOnlyFirstAppeared = false;
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.WaitingLists.MAIN,
                null,
                trackCurrentView: true);
        }

        private void ShowItemOperations(bool showItemOperationWait)
        {
            var missionOperation = this.missionOperationsService.CurrentMissionOperation;
            if (missionOperation != null)
            {
                switch (missionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.checkOnlyFirstAppeared = false;
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.INVENTORY,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Pick:
                        this.checkOnlyFirstAppeared = false;
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PICK,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Put:
                        this.checkOnlyFirstAppeared = false;
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PUT,
                            null,
                            trackCurrentView: true);
                        break;
                }
            }
            else
            {
                if (showItemOperationWait)
                {
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.ItemOperations.WAIT,
                        null,
                        trackCurrentView: true);
                }
            }
        }

        private void ShowItemSearch()
        {
            this.checkOnlyFirstAppeared = false;
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.MAIN,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
