using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerWaitViewModel : BaseViewModel, IDrawerWaitViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMainWindowViewModel mainWindowViewModel;

        private readonly INavigationService navigationService;

        private string waitingMissions;

        #endregion

        #region Constructors

        public DrawerWaitViewModel(
            IEventAggregator eventAggregator,
            IBayManager bayManager,
            INavigationService navigationService,
            IMainWindowViewModel mainWindowViewModel)
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

            if (mainWindowViewModel == null)
            {
                throw new System.ArgumentNullException(nameof(mainWindowViewModel));
            }

            this.eventAggregator = eventAggregator;
            this.navigationService = navigationService;
            this.bayManager = bayManager;
            this.mainWindowViewModel = mainWindowViewModel;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public string WaitingMissions { get => this.waitingMissions; set => this.SetProperty(ref this.waitingMissions, value); }

        #endregion

        #region Methods

       
        public override async Task OnEnterViewAsync()
        {
            this.WaitingMissions = this.bayManager.QueuedMissionsQuantity.ToString();
        }

        public void UpdateView()
        {
            var mission = this.bayManager.CurrentMission;
            var mainWindowContentVM = this.mainWindowViewModel.ContentRegionCurrentViewModel;
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
                            this.navigationService
                                .NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionType.Pick:
                            this.navigationService
                                .NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionType.Put:
                            this.navigationService
                                .NavigateToViewWithoutNavigationStack<DrawerActivityRefillingViewModel, IDrawerActivityRefillingViewModel>();
                            break;
                    }
                }
                else
                {
                    this.navigationService
                        .NavigateToViewWithoutNavigationStack<DrawerWaitViewModel, IDrawerWaitViewModel>();
                }
            }
        }

        #endregion
    }
}
