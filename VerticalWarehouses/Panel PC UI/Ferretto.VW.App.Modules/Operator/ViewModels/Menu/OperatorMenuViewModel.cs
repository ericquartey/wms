using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class OperatorMenuViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineBaysWebService machineBaysWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private bool areItemsEnabled;

        private DelegateCommand drawerActivityButtonCommand;

        private bool isWaitingForResponse;

        private DelegateCommand showItemListsCommand;

        private DelegateCommand showItemSearchCommand;

        private DelegateCommand showOtherMenuCommand;

        #endregion

        #region Constructors

        public OperatorMenuViewModel(
            IMachineBaysWebService machineBaysWebService,
            IMissionOperationsService missionOperationsService)
            : base(PresentationMode.Operator)
        {
            this.machineBaysWebService = machineBaysWebService ?? throw new ArgumentNullException(nameof(machineBaysWebService));
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
        }

        #endregion

        #region Properties

        public bool AreItemsEnabled
        {
            get => this.areItemsEnabled;
            private set => this.SetProperty(ref this.areItemsEnabled, value);
        }

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand
            ??
            (this.drawerActivityButtonCommand = new DelegateCommand(
                this.ShowItemOperations,
                this.CanShowItemOperations));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
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

        public ICommand ShowOtherMenuCommand =>
            this.showOtherMenuCommand
            ??
            (this.showOtherMenuCommand = new DelegateCommand(
                this.ShowOtherMenu,
                this.CanShowOtherMenu));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            await this.machineBaysWebService.ActivateAsync();
        }

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            this.AreItemsEnabled = e.MachinePowerState is MachinePowerState.Powered;
        }

        private bool CanShowItemLists()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanShowItemOperations()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanShowItemSearch()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanShowOtherMenu()
        {
            return !this.IsWaitingForResponse;
        }

        private void ShowItemLists()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.WaitingLists.MAIN,
                null,
                trackCurrentView: true);
        }

        private void ShowItemOperations()
        {
            var missionOperation = this.missionOperationsService.CurrentMissionOperation;
            if (missionOperation != null)
            {
                switch (missionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.INVENTORY,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Pick:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.ItemOperations.PICK,
                            null,
                            trackCurrentView: true);
                        break;

                    case MissionOperationType.Put:
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
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemOperations.WAIT,
                    null,
                    trackCurrentView: true);
            }
        }

        private void ShowItemSearch()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.ItemSearch.MAIN,
                null,
                trackCurrentView: true);
        }

        private void ShowOtherMenu()
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Operator),
                Utils.Modules.Operator.Others.NAVIGATION,
                null,
                trackCurrentView: true);
        }

        #endregion
    }
}
