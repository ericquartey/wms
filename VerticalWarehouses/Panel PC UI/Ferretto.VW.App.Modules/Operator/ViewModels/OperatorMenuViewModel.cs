using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
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

        public override EnableMask EnableMask => EnableMask.None;

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

        protected override void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            base.OnMachineModeChanged(e);

            this.AreItemsEnabled = e.MachinePower != Services.Models.MachinePowerState.Unpowered;
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
                                Utils.Modules.Operator.EMPTY,
                                null,
                                trackCurrentView: true);
                            //this.navigationService.NavigateToView<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionOperationType.Pick:
                            this.NavigationService.Appear(
                                nameof(Utils.Modules.Operator),
                                Utils.Modules.Operator.EMPTY,
                                null,
                                trackCurrentView: true);
                            //this.navigationService.NavigateToView<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionOperationType.Put:
                            this.NavigationService.Appear(
                                nameof(Utils.Modules.Operator),
                                Utils.Modules.Operator.EMPTY,
                                null,
                                trackCurrentView: true);
                            //this.navigationService.NavigateToView<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;
                    }
                }
                else
                {
                    this.NavigationService.Appear(
                        nameof(Utils.Modules.Operator),
                        Utils.Modules.Operator.EMPTY,
                        null,
                        trackCurrentView: true);
                    //this.navigationService.NavigateToView<DrawerWaitViewModel, IDrawerWaitViewModel>();
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
                    Utils.Modules.Operator.EMPTY,
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
                    Utils.Modules.Operator.EMPTY,
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
                    Utils.Modules.Operator.EMPTY,
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
