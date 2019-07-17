using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationStateViewModel : BindableBase, IInstallationStateViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private IInstallationStatusService installationStatusService;

        private bool isBeltBurnishingDone;

        private bool isCellPositionVerifyDone;

        private bool isCheckPanelsDone;

        private bool isEmptyDrawersLoadedDone;

        private bool isFirstDrawerLoadedDone;

        private bool isHorizontalHomingDone;

        private bool isLaserShutter1Done;

        private bool isLaserShutter2Done;

        private bool isLaserShutter3Done;

        private bool isMachineDone;

        private bool isShapeShutter1Done;

        private bool isShapeShutter2Done;

        private bool isShapeShutter3Done;

        private bool isShutter1InstallationProcedureDone;

        private bool isShutter2InstallationProcedureDone;

        private bool isShutter3InstallationProcedureDone;

        private bool isVerticalHomingDone;

        private bool isVerticalOffsetVerifyDone;

        private bool isVerticalResolutionDone;

        #endregion

        #region Constructors

        public InstallationStateViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        public InstallationStateViewModel()
        {
            // TODO
        }

        #endregion

        #region Properties

        public bool IsBeltBurnishingDone { get => this.isBeltBurnishingDone; set => this.SetProperty(ref this.isBeltBurnishingDone, value); }

        public bool IsCellPositionVerifyDone { get => this.isCellPositionVerifyDone; set => this.SetProperty(ref this.isCellPositionVerifyDone, value); }

        public bool IsCheckPanelsVerifyDone { get => this.isCheckPanelsDone; set => this.SetProperty(ref this.isCheckPanelsDone, value); }

        public bool IsEmptyDrawersLoadedDone { get => this.isEmptyDrawersLoadedDone; set => this.SetProperty(ref this.isEmptyDrawersLoadedDone, value); }

        public bool IsFirstDrawerLoadedDone { get => this.isFirstDrawerLoadedDone; set => this.SetProperty(ref this.isFirstDrawerLoadedDone, value); }

        public bool IsHorizontalHomingDone { get => this.isHorizontalHomingDone; set => this.SetProperty(ref this.isHorizontalHomingDone, value); }

        public bool IsLaserShutter1Done { get => this.isLaserShutter1Done; set => this.SetProperty(ref this.isLaserShutter1Done, value); }

        public bool IsLaserShutter2Done { get => this.isLaserShutter2Done; set => this.SetProperty(ref this.isLaserShutter2Done, value); }

        public bool IsLaserShutter3Done { get => this.isLaserShutter3Done; set => this.SetProperty(ref this.isLaserShutter3Done, value); }

        public bool IsMachineDone { get => this.isMachineDone; set => this.SetProperty(ref this.isMachineDone, value); }

        public bool IsShapeShutter1Done { get => this.isShapeShutter1Done; set => this.SetProperty(ref this.isShapeShutter1Done, value); }

        public bool IsShapeShutter2Done { get => this.isShapeShutter2Done; set => this.SetProperty(ref this.isShapeShutter2Done, value); }

        public bool IsShapeShutter3Done { get => this.isShapeShutter3Done; set => this.SetProperty(ref this.isShapeShutter3Done, value); }

        public bool IsShutter1InstallationProcedureDone { get => this.isShutter1InstallationProcedureDone; set => this.SetProperty(ref this.isShutter1InstallationProcedureDone, value); }

        public bool IsShutter2InstallationProcedureDone { get => this.isShutter2InstallationProcedureDone; set => this.SetProperty(ref this.isShutter2InstallationProcedureDone, value); }

        public bool IsShutter3InstallationProcedureDone { get => this.isShutter3InstallationProcedureDone; set => this.SetProperty(ref this.isShutter3InstallationProcedureDone, value); }

        public bool IsVerticalHomingDone { get => this.isVerticalHomingDone; set => this.SetProperty(ref this.isVerticalHomingDone, value); }

        public bool IsVerticalOffsetVerifyDone { get => this.isVerticalOffsetVerifyDone; set => this.SetProperty(ref this.isVerticalOffsetVerifyDone, value); }

        public bool IsVerticalResolutionDone { get => this.isVerticalResolutionDone; set => this.SetProperty(ref this.isVerticalResolutionDone, value); }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationStatusService = this.container.Resolve<IInstallationStatusService>();
        }

        public async Task OnEnterViewAsync()
        {
            await this.GetInstallationStateAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private async Task GetInstallationStateAsync()
        {
            var installationStatus = (await this.installationStatusService.GetStatusAsync()).ToArray();

            this.IsVerticalHomingDone = installationStatus[0];
            this.IsHorizontalHomingDone = installationStatus[1];
            this.IsBeltBurnishingDone = installationStatus[2];
            this.IsVerticalResolutionDone = installationStatus[3];
            this.IsVerticalOffsetVerifyDone = installationStatus[4];
            this.IsCellPositionVerifyDone = installationStatus[5];
            this.IsShutter1InstallationProcedureDone = installationStatus[11];
            this.IsShutter2InstallationProcedureDone = installationStatus[12];
            this.IsShutter3InstallationProcedureDone = installationStatus[13];
            this.IsShapeShutter1Done = installationStatus[7];
            this.IsShapeShutter2Done = installationStatus[8];
            this.IsShapeShutter3Done = installationStatus[9];
            this.IsLaserShutter1Done = installationStatus[19];
            this.IsLaserShutter2Done = installationStatus[20];
            this.IsLaserShutter3Done = installationStatus[21];
            this.IsFirstDrawerLoadedDone = installationStatus[17];
            this.IsEmptyDrawersLoadedDone = installationStatus[18];
            this.IsCheckPanelsVerifyDone = installationStatus[6];

            var checkMachineDone = true;
            foreach (var itemState in installationStatus)
            {
                checkMachineDone = itemState && checkMachineDone;
            }

            this.IsMachineDone = checkMachineDone;
        }

        #endregion
    }
}
