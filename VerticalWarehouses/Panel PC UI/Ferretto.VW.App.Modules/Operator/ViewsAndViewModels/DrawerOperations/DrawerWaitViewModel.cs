using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Modules.Operator.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using INavigationService = Ferretto.VW.App.Modules.Operator.Interfaces.INavigationService;

namespace Ferretto.VW.App.Modules.Operator.ViewsAndViewModels.DrawerOperations
{
    public class DrawerWaitViewModel : BaseViewModel, IDrawerWaitViewModel, IDrawerActivityViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly IMainWindowViewModel mainWindowViewModel;

        private readonly INavigationService navigationService;

        private int pendingMissionsCount;

        #endregion

        #region Constructors

        public DrawerWaitViewModel(
            IBayManager bayManager,
            INavigationService navigationService,
            IMainWindowViewModel mainWindowViewModel)
        {
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

            this.bayManager.NewMissionOperationAvailable += this.OnMissionOperationAvailable;
            this.UpdateView();

            return Task.CompletedTask;
        }

        public void UpdateView()
        {
            this.PendingMissionsCount = this.bayManager.PendingMissionsCount;

            var missionOperation = this.bayManager.CurrentMissionOperation;

            var currentViewModel = this.mainWindowViewModel.ContentRegionCurrentViewModel;
            if (currentViewModel is DrawerActivityInventoryViewModel ||
                currentViewModel is DrawerActivityPickingViewModel ||
                currentViewModel is DrawerActivityRefillingViewModel ||
                currentViewModel is DrawerWaitViewModel)
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

        private void OnMissionOperationAvailable(object sender, object e)
        {
            this.UpdateView();
        }

        #endregion
    }
}
