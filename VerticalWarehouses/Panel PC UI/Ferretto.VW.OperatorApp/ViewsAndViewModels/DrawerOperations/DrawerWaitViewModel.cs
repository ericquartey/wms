using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations
{
    public class DrawerWaitViewModel : BaseViewModel, IDrawerWaitViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IEventAggregator eventAggregator;

        private readonly IMainWindowViewModel mainWindowViewModel;

        private readonly INavigationService navigationService;

        private int pendingMissionsCount;

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
        }

        #endregion

        #region Properties

        public int PendingMissionsCount
        {
            get => this.pendingMissionsCount;
            set => this.SetProperty(ref this.pendingMissionsCount, value);
        }

        #endregion

        #region Methods

        public override Task OnEnterViewAsync()
        {
            this.PendingMissionsCount = this.bayManager.PendingMissionsCount;

            return Task.CompletedTask;
        }

        public void UpdateView()
        {
            var missionOperation = this.bayManager.CurrentMissionOperation;
            var mainWindowContentVM = this.mainWindowViewModel.ContentRegionCurrentViewModel;
            if (mainWindowContentVM is DrawerActivityInventoryViewModel ||
                mainWindowContentVM is DrawerActivityPickingViewModel ||
                mainWindowContentVM is DrawerActivityRefillingViewModel ||
                mainWindowContentVM is DrawerWaitViewModel)
            {
                if (missionOperation != null)
                {
                    switch (missionOperation.Type)
                    {
                        case MissionOperationType.Inventory:
                            this.navigationService
                                .NavigateToViewWithoutNavigationStack<DrawerActivityInventoryViewModel, IDrawerActivityInventoryViewModel>();
                            break;

                        case MissionOperationType.Pick:
                            this.navigationService
                                .NavigateToViewWithoutNavigationStack<DrawerActivityPickingViewModel, IDrawerActivityPickingViewModel>();
                            break;

                        case MissionOperationType.Put:
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
