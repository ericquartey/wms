using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class DrawerWaitViewModel : BaseDrawerOperationViewModel
    {
        #region Fields

        private int pendingMissionsCount;

        #endregion

        #region Constructors

        public DrawerWaitViewModel(
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

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.PendingMissionsCount = this.BayManager.PendingMissionsCount;

            this.BayManager.NewMissionOperationAvailable += this.OnMissionOperationAvailable;

            this.UpdateView();
        }

        public override void UpdateView()
        {
            base.UpdateView();
            this.PendingMissionsCount = this.BayManager.PendingMissionsCount;
        }

        private void OnMissionOperationAvailable(object sender, object e)
        {
            this.UpdateView();
        }

        #endregion
    }
}
