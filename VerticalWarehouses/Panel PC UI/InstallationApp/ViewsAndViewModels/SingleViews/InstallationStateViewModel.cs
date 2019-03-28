 using Prism.Mvvm;
using Ferretto.VW.Utils.Source;
using System;
using Microsoft.Practices.Unity;
using Prism.Events;
using System.Net.Http;
using System.Configuration;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationStateViewModel : BindableBase, IInstallationStateViewModel
    {
        #region Fields

        private string installationController = ConfigurationManager.AppSettings.Get("InstallationController");

        private string getInstallationStatus = ConfigurationManager.AppSettings.Get("GetInstallationStatus");

        public IUnityContainer Container;

        public DataManager Data;

        private IEventAggregator eventAggregator;

        private bool isBeltBurnishingDone;

        private bool isCellPositionVerifyDone;

        private bool isHorizontalHomingDone;

        private bool isLaserShutter1Done;

        private bool isLaserShutter2Done;

        private bool isLaserShutter3Done;

        private bool isMachineDone;

        private bool isVerticalOffsetVerifyDone;

        private bool isVerticalResolutionDone;

        private bool isShapeShutter1Done;

        private bool isShapeShutter2Done;

        private bool isShapeShutter3Done;

        private bool isShutter1InstallationProcedureDone;

        private bool isShutter2InstallationProcedureDone;

        private bool isShutter3InstallationProcedureDone;

        private bool isVerticalHomingDone;

        #endregion

        #region Constructors

        public InstallationStateViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        public InstallationStateViewModel()
        {
            // TODO
        }

        #endregion

        #region Properties

        public bool IsBeltBurnishingDone { get => this.isBeltBurnishingDone; set => this.SetProperty(ref this.isBeltBurnishingDone, value); }

        public bool IsCellPositionVerifyDone { get => this.isCellPositionVerifyDone; set => this.SetProperty(ref this.isCellPositionVerifyDone, value); }

        public bool IsHorizontalHomingDone { get => this.isHorizontalHomingDone; set => this.SetProperty(ref this.isHorizontalHomingDone, value); }

        public bool IsLaserShutter1Done { get => this.isLaserShutter1Done; set => this.SetProperty(ref this.isLaserShutter1Done, value); }

        public bool IsLaserShutter2Done { get => this.isLaserShutter2Done; set => this.SetProperty(ref this.isLaserShutter2Done, value); }

        public bool IsLaserShutter3Done { get => this.isLaserShutter3Done; set => this.SetProperty(ref this.isLaserShutter3Done, value); }

        public bool IsMachineDone { get => this.isMachineDone; set => this.SetProperty(ref this.isMachineDone, value); }

        public bool IsVerticalOffsetVerifyDone { get => this.isVerticalOffsetVerifyDone; set => this.SetProperty(ref this.isVerticalOffsetVerifyDone, value); }

        public bool IsVerticalResolutionDone { get => this.isVerticalResolutionDone; set => this.SetProperty(ref this.isVerticalResolutionDone, value); }

        public bool IsShapeShutter1Done { get => this.isShapeShutter1Done; set => this.SetProperty(ref this.isShapeShutter1Done, value); }

        public bool IsShapeShutter2Done { get => this.isShapeShutter2Done; set => this.SetProperty(ref this.isShapeShutter2Done, value); }

        public bool IsShapeShutter3Done { get => this.isShapeShutter3Done; set => this.SetProperty(ref this.isShapeShutter3Done, value); }

        public bool IsShutter1InstallationProcedureDone { get => this.isShutter1InstallationProcedureDone; set => this.SetProperty(ref this.isShutter1InstallationProcedureDone, value); }

        public bool IsShutter2InstallationProcedureDone { get => this.isShutter2InstallationProcedureDone; set => this.SetProperty(ref this.isShutter2InstallationProcedureDone, value); }

        public bool IsShutter3InstallationProcedureDone { get => this.isShutter3InstallationProcedureDone; set => this.SetProperty(ref this.isShutter3InstallationProcedureDone, value); }

        public bool IsVerticalHomingDone { get => this.isVerticalHomingDone; set => this.SetProperty(ref this.isVerticalHomingDone, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public async void GetInstallationState()
        {
            var client = new HttpClient();
            var response = await client.GetAsync(new Uri(this.installationController + this.getInstallationStatus));

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var installationStatus = response.Content.ReadAsAsync<bool[]>().Result;
                this.IsVerticalHomingDone = installationStatus[0];
                this.IsHorizontalHomingDone = installationStatus[1];
                this.IsBeltBurnishingDone = installationStatus[2];
                this.IsVerticalResolutionDone = installationStatus[3];
                this.IsVerticalOffsetVerifyDone = installationStatus[4];
                this.IsCellPositionVerifyDone = installationStatus[5];
                this.IsShutter1InstallationProcedureDone = installationStatus[6];
                this.IsShutter2InstallationProcedureDone = installationStatus[7];
                this.IsShutter3InstallationProcedureDone = installationStatus[8];
                this.IsShapeShutter1Done = installationStatus[9];
                this.IsShapeShutter2Done = installationStatus[10];
                this.IsShapeShutter3Done = installationStatus[11];
                this.IsLaserShutter1Done = installationStatus[12];
                this.IsLaserShutter2Done = installationStatus[13];
                this.IsLaserShutter3Done = installationStatus[14];
            }
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.Container = container;
        }

        public async void SubscribeMethodToEvent()
        {
            this.GetInstallationState();
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
