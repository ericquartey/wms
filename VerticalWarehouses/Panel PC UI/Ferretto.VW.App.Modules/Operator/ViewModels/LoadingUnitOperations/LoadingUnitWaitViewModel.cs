using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitWaitViewModel : BaseLoadingUnitOperationViewModel
    {
        #region Fields

        private int pendingMissionsCount;

        #endregion

        #region Constructors

        public LoadingUnitWaitViewModel(
            IWmsDataProvider wmsDataProvider,
            IWmsImagesProvider wmsImagesProvider,
            IMachineMissionOperationsWebService missionOperationsService,
            IBayManager bayManager)
            : base(wmsDataProvider, wmsImagesProvider, missionOperationsService, bayManager)
        {
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public int PendingMissionsCount
        {
            get => this.pendingMissionsCount;
            set => this.SetProperty(ref this.pendingMissionsCount, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.BayManager.NewMissionOperationAvailable -= this.OnMissionOperationAvailable;
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.PendingMissionsCount = this.BayManager.PendingMissionsCount;

            this.BayManager.NewMissionOperationAvailable += this.OnMissionOperationAvailable;

            this.UpdateOperation();

            this.PendingMissionsCount = this.BayManager.PendingMissionsCount;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public virtual void UpdateOperation()
        {
            var missionOperation = this.BayManager.CurrentMissionOperation;
            if (missionOperation != null)
            {
                switch (missionOperation.Type)
                {
                    case MissionOperationType.Inventory:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.DrawerOperations.INVENTORY,
                            null,
                            trackCurrentView: false);
                        break;

                    case MissionOperationType.Pick:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.DrawerOperations.PICKING,
                            null,
                            trackCurrentView: false);
                        break;

                    case MissionOperationType.Put:
                        this.NavigationService.Appear(
                            nameof(Utils.Modules.Operator),
                            Utils.Modules.Operator.DrawerOperations.REFILLING,
                            null,
                            trackCurrentView: false);
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

        private void OnMissionOperationAvailable(object sender, object e)
        {
            this.UpdateOperation();
        }

        #endregion
    }
}
