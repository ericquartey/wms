using System.Windows.Input;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Operator.Interfaces;
using Ferretto.VW.App.Operator.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.App.Operator.ViewsAndViewModels.Other;
using Ferretto.VW.App.Operator.ViewsAndViewModels.SearchItem;
using Ferretto.VW.App.Operator.ViewsAndViewModels.WaitingLists;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels
{
    public class MainWindowNavigationButtonsViewModel : BaseViewModel, IMainWindowNavigationButtonsViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly Ferretto.VW.App.Operator.Interfaces.INavigationService navigationService;

        private ICommand drawerActivityButtonCommand;

        private ICommand itemSearchButtonCommand;

        private ICommand listsInWaitButtonCommand;

        private ICommand otherButtonCommand;

        #endregion

        #region Constructors

        public MainWindowNavigationButtonsViewModel(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            Ferretto.VW.App.Operator.Interfaces.INavigationService navigationService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (bayManager == null)
            {
                throw new System.ArgumentNullException(nameof(bayManager));
            }

            if (navigationService == null)
            {
                throw new System.ArgumentNullException(nameof(navigationService));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.bayManager = bayManager;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand ?? (this.drawerActivityButtonCommand = new DelegateCommand(() =>
        {
            this.DrawerActivityButtonMethod();
        }));

        public ICommand ItemSearchButtonCommand => this.itemSearchButtonCommand ?? (this.itemSearchButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<ItemSearchViewModel, IItemSearchViewModel>();
        }));

        public ICommand ListsInWaitButtonCommand => this.listsInWaitButtonCommand ?? (this.listsInWaitButtonCommand = new DelegateCommand(() =>
        {
            this.navigationService.NavigateToView<ListsInWaitViewModel, IListsInWaitViewModel>();
        }));

        public ICommand OtherButtonCommand => this.otherButtonCommand ?? (this.otherButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<GeneralInfoViewModel, IGeneralInfoViewModel>();
                }));

        #endregion

        #region Methods

        public void DrawerActivityButtonMethod()
        {
            var missionOperation = this.bayManager.CurrentMissionOperation;
            if (missionOperation != null)
            {
                switch (missionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.navigationService.NavigateToView<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                        break;

                    case MissionOperationType.Pick:
                        this.navigationService.NavigateToView<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                        break;

                    case MissionOperationType.Put:
                        this.navigationService.NavigateToView<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                        break;
                }
            }
            else
            {
                this.navigationService.NavigateToView<DrawerWaitViewModel, IDrawerWaitViewModel>();
            }
        }

        #endregion
    }
}
