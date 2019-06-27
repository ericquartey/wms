using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using System.Windows.Input;
using Prism.Commands;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.SearchItem;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.WaitingLists;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.Other;
using Unity;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels
{
    public class MainWindowNavigationButtonsViewModel : BindableBase, IMainWindowNavigationButtonsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private ICommand drawerActivityButtonCommand;

        private ICommand itemSearchButtonCommand;

        private ICommand listsInWaitButtonCommand;

        private ICommand otherButtonCommand;

        #endregion

        #region Constructors

        public MainWindowNavigationButtonsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public ICommand DrawerActivityButtonCommand => this.drawerActivityButtonCommand ?? (this.drawerActivityButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
        }));

        public ICommand ItemSearchButtonCommand => this.itemSearchButtonCommand ?? (this.itemSearchButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<ItemSearchViewModel, IItemSearchViewModel>();
        }));

        public ICommand ListsInWaitButtonCommand => this.listsInWaitButtonCommand ?? (this.listsInWaitButtonCommand = new DelegateCommand(() =>
        {
            NavigationService.NavigateToView<ListsInWaitViewModel, IListsInWaitViewModel>();
        }));

        public BindableBase NavigationViewModel { get; set; }

        public ICommand OtherButtonCommand => this.otherButtonCommand ?? (this.otherButtonCommand = new DelegateCommand(() =>
                {
                    NavigationService.NavigateToView<GeneralInfoViewModel, IGeneralInfoViewModel>();
                }));

        #endregion

        #region Methods

        public async void DrawerActivityButtonMethod()
        {
            var mission = this.container.Resolve<IBayManager>().CurrentMission;
            if (mission != null)
            {
                switch (mission.Type)
                {
                    case MissionType.Inventory:
                        NavigationService.NavigateToView<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                        break;

                    case MissionType.Pick:
                        NavigationService.NavigateToView<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                        break;

                    case MissionType.Put:
                        NavigationService.NavigateToView<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                        break;
                }
            }
            else
            {
                NavigationService.NavigateToView<DrawerWaitViewModel, IDrawerWaitViewModel>();
            }
        }

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
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
