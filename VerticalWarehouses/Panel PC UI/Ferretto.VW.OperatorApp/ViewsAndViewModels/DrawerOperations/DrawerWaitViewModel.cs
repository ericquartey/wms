using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerWaitViewModel : BindableBase, IDrawerWaitViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        private string waitingMissions;

        #endregion

        #region Constructors

        public DrawerWaitViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public string WaitingMissions { get => this.waitingMissions; set => this.SetProperty(ref this.waitingMissions, value); }

        #endregion

        #region Methods

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
            this.WaitingMissions = this.container.Resolve<IBayManager>().QueuedMissionsQuantity.ToString();
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        public void UpdateView()
        {
            var mission = this.container.Resolve<IBayManager>().CurrentMission;
            var mainWindowContentVM = this.container.Resolve<IMainWindowViewModel>().ContentRegionCurrentViewModel;
            if (mainWindowContentVM is DrawerActivityInventoryViewModel ||
                mainWindowContentVM is DrawerActivityPickingViewModel ||
                mainWindowContentVM is DrawerActivityRefillingViewModel ||
                mainWindowContentVM is DrawerWaitViewModel)
            {
                if (mission != null)
                {
                    switch (mission.Type)
                    {
                        case MissionType.Inventory:
                            NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionType.Pick:
                            NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionType.Put:
                            NavigationService.NavigateToViewWithoutNavigationStack<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;
                    }
                }
                else
                {
                    NavigationService.NavigateToViewWithoutNavigationStack<DrawerWaitViewModel, IDrawerWaitViewModel>();
                }
            }
        }

        #endregion
    }
}
