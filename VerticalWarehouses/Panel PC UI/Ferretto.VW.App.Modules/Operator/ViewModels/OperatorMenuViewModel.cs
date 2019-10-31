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
    public class OperatorMenuViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private bool areItemsEnabled;

        private DelegateCommand drawerActivityButtonCommand;

        private bool isWaitingForResponse;

        private DelegateCommand itemSearchButtonCommand;

        private DelegateCommand listsInWaitButtonCommand;

        private DelegateCommand otherButtonCommand;

        #endregion

        #region Constructors

        public OperatorMenuViewModel(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            INavigationService navigationService)
            : base(PresentationMode.Operator)
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        }

        #endregion

        #region Properties

        public bool AreItemsEnabled
        {
            get => this.areItemsEnabled;
            private set => this.SetProperty(ref this.areItemsEnabled, value);
        }

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand ?? (this.drawerActivityButtonCommand = new DelegateCommand(() => this.DrawerActivityButtonMethod(), this.CanDrawerActivityButtonMethod));

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

        public ICommand ItemSearchButtonCommand => this.itemSearchButtonCommand ?? (this.itemSearchButtonCommand = new DelegateCommand(() => this.ItemSearch(), this.CanItemSearchCommand));

        public ICommand ListsInWaitButtonCommand => this.listsInWaitButtonCommand ?? (this.listsInWaitButtonCommand = new DelegateCommand(() => this.ListInWait(), this.CanListInWaitCommand));

        public ICommand OtherButtonCommand => this.otherButtonCommand ?? (this.otherButtonCommand = new DelegateCommand(() => this.Other(), this.CanOtherCommand));

        #endregion

        #region Methods

        protected override async Task OnMachinePowerChangedAsync(MachinePowerChangedEventArgs e)
        {
            await base.OnMachinePowerChangedAsync(e);

            this.AreItemsEnabled = e.MachinePowerState is MachinePowerState.Powered;
        }

        private bool CanDrawerActivityButtonMethod()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanItemSearchCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanListInWaitCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private bool CanOtherCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void DrawerActivityButtonMethod()
        {
            this.IsWaitingForResponse = true;

            try
            {
                var missionOperation = this.bayManager.CurrentMissionOperation;
                if (missionOperation != null)
                {
                    switch (missionOperation.Type)
                    {
                        case MissionOperationType.Inventory:
                            this.NavigationService.Appear(
                                nameof(Utils.Modules.Operator),
                                Utils.Modules.Operator.DrawerOperations.INVENTORY,
                                null,
                                trackCurrentView: true);
                            break;

                        case MissionOperationType.Pick:
                            this.NavigationService.Appear(
                                nameof(Utils.Modules.Operator),
                                Utils.Modules.Operator.DrawerOperations.PICKING,
                                null,
                                trackCurrentView: true);
                            break;

                        case MissionOperationType.Put:
                            this.NavigationService.Appear(
                                nameof(Utils.Modules.Operator),
                                Utils.Modules.Operator.DrawerOperations.REFILLING,
                                null,
                                trackCurrentView: true);
                            break;
                    }
                }
                else
                {
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.DrawerOperations.WAIT,
                        null,
                        trackCurrentView: true);
                }
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void ItemSearch()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.ItemSearch.MAIN,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void ListInWait()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.WaitingLists.MAIN,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void Other()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.NAVIGATION,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
