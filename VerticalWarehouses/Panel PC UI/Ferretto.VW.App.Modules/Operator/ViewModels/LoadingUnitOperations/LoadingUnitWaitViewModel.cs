using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Regions;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class LoadingUnitWaitViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private int pendingMissionsCount;

        #endregion

        #region Constructors

        public LoadingUnitWaitViewModel(
                IBayManager bayManager)
            : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public int PendingMissionOperationsCount
        {
            get => this.pendingMissionsCount;
            set => this.SetProperty(ref this.pendingMissionsCount, value);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.bayManager.NewMissionOperationAvailable -= this.OnMissionOperationAvailable;
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.bayManager.NewMissionOperationAvailable += this.OnMissionOperationAvailable;

            this.UpdateOperation();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public virtual void UpdateOperation()
        {
            this.PendingMissionOperationsCount = this.bayManager.PendingMissionOperationsCount;
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
        }

        private void OnMissionOperationAvailable(object sender, object e)
        {
            this.UpdateOperation();
        }

        #endregion
    }
}
