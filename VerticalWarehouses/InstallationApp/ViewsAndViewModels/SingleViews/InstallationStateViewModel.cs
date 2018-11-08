using Prism.Mvvm;
using Ferretto.VW.Utils.Source;
using System;
using Ferretto.VW.Navigation;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SingleViews
{
    internal class InstallationStateViewModel : BindableBase
    {
        #region Fields

        private bool isBeltBurnishingDone;
        private bool isGate1Done;
        private bool isGate2Done;
        private bool isGate3Done;
        private bool isHorizontalHomingDone = false;
        private bool isLaserGate1Done;
        private bool isLaserGate2Done;
        private bool isLaserGate3Done;
        private bool isMachineDone;
        private bool isSetResolutionDone;
        private bool isShapeGate1Done;
        private bool isShapeGate2Done;
        private bool isShapeGate3Done;
        private bool isVerticalHomingDone = false;

        #endregion Fields

        #region Constructors

        public InstallationStateViewModel()
        {
            this.UpdateData();
            NavigationService.InstallationInfoChangedEventHandler += this.UpdateData;
        }

        #endregion Constructors

        #region Properties

        public Boolean IsBeltBurnishingDone { get => this.isBeltBurnishingDone; set => this.SetProperty(ref this.isBeltBurnishingDone, value); }
        public Boolean IsGate1Done { get => this.isGate1Done; set => this.SetProperty(ref this.isGate1Done, value); }
        public Boolean IsGate2Done { get => this.isGate2Done; set => this.SetProperty(ref this.isGate2Done, value); }
        public Boolean IsGate3Done { get => this.isGate3Done; set => this.SetProperty(ref this.isGate3Done, value); }
        public Boolean IsHorizontalHomingDone { get => this.isHorizontalHomingDone; set => this.SetProperty(ref this.isHorizontalHomingDone, value); }
        public Boolean IsLaserGate1Done { get => this.isLaserGate1Done; set => this.SetProperty(ref this.isLaserGate1Done, value); }
        public Boolean IsLaserGate2Done { get => this.isLaserGate2Done; set => this.SetProperty(ref this.isLaserGate2Done, value); }
        public Boolean IsLaserGate3Done { get => this.isLaserGate3Done; set => this.SetProperty(ref this.isLaserGate3Done, value); }
        public Boolean IsMachineDone { get => this.isMachineDone; set => this.SetProperty(ref this.isMachineDone, value); }
        public Boolean IsSetResolutionDone { get => this.isSetResolutionDone; set => this.SetProperty(ref this.isSetResolutionDone, value); }
        public Boolean IsShapeGate1Done { get => this.isShapeGate1Done; set => this.SetProperty(ref this.isShapeGate1Done, value); }
        public Boolean IsShapeGate2Done { get => this.isShapeGate2Done; set => this.SetProperty(ref this.isShapeGate2Done, value); }
        public Boolean IsShapeGate3Done { get => this.isShapeGate3Done; set => this.SetProperty(ref this.isShapeGate3Done, value); }
        public Boolean IsVerticalHomingDone { get => this.isVerticalHomingDone; set => this.SetProperty(ref this.isVerticalHomingDone, value); }

        #endregion Properties

        #region Methods

        private void UpdateData()
        {
            this.IsBeltBurnishingDone = DataManager.CurrentData.InstallationInfo.Belt_Burnishing;
            this.IsGate1Done = DataManager.CurrentData.InstallationInfo.Ok_Gate1;
            this.IsGate2Done = DataManager.CurrentData.InstallationInfo.Ok_Gate2;
            this.IsGate3Done = DataManager.CurrentData.InstallationInfo.Ok_Gate3;
            this.IsHorizontalHomingDone = DataManager.CurrentData.InstallationInfo.Origin_X_Axis;
            this.IsLaserGate1Done = DataManager.CurrentData.InstallationInfo.Ok_Laser1;
            this.IsLaserGate2Done = DataManager.CurrentData.InstallationInfo.Ok_Laser2;
            this.IsLaserGate3Done = DataManager.CurrentData.InstallationInfo.Ok_Laser3;
            this.IsMachineDone = DataManager.CurrentData.InstallationInfo.Machine_Ok;
            this.IsShapeGate1Done = DataManager.CurrentData.InstallationInfo.Ok_Shape1;
            this.IsShapeGate2Done = DataManager.CurrentData.InstallationInfo.Ok_Shape2;
            this.IsShapeGate3Done = DataManager.CurrentData.InstallationInfo.Ok_Shape3;
            this.IsSetResolutionDone = DataManager.CurrentData.InstallationInfo.Set_Y_Resolution;
            this.IsVerticalHomingDone = DataManager.CurrentData.InstallationInfo.Origin_Y_Axis;
        }

        #endregion Methods
    }
}
