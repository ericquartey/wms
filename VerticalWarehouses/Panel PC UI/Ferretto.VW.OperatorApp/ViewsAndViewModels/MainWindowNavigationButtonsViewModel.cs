using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class MainWindowNavigationButtonsViewModel : BindableBase, IMainWindowNavigationButtonsViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly INavigationService navigationService;

        private ICommand drawerActivityButtonCommand;

        private ICommand itemSearchButtonCommand;

        private ICommand listsInWaitButtonCommand;

        private ICommand otherButtonCommand;

        #endregion

        #region Constructors

        public MainWindowNavigationButtonsViewModel(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            INavigationService navigationService)
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

        public BindableBase NavigationViewModel { get; set; }

        public ICommand OtherButtonCommand => this.otherButtonCommand ?? (this.otherButtonCommand = new DelegateCommand(() =>
                {
                    this.navigationService.NavigateToView<GeneralInfoViewModel, IGeneralInfoViewModel>();
                }));

        #endregion

        #region Methods

        public async void DrawerActivityButtonMethod()
        {
            var mission = this.bayManager.CurrentMission;
            if (mission != null)
            {
                switch (mission.Type)
                {
                    case MissionType.Inventory:
                        this.navigationService.NavigateToView<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                        break;

                    case MissionType.Pick:
                        this.navigationService.NavigateToView<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                        break;

                    case MissionType.Put:
                        this.navigationService.NavigateToView<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                        break;
                }
            }
            else
            {
                this.navigationService.NavigateToView<DrawerWaitViewModel, IDrawerWaitViewModel>();
            }
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
