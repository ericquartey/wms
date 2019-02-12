using Prism.Mvvm;
using Ferretto.VW.Utils.Source;
using System;
using Ferretto.VW.Navigation;
using Microsoft.Practices.Unity;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationStateViewModel : BindableBase, IViewModel, IInstallationStateViewModel
    {
        #region Fields

        public IUnityContainer Container;

        public DataManager Data;

        private bool isBeltBurnishingDone;

        private bool isCellPositionDone;

        private bool isGate1Done;

        private bool isGate2Done;

        private bool isGate3Done;

        private bool isHorizontalHomingDone = false;

        private bool isLaserGate1Done;

        private bool isLaserGate2Done;

        private bool isLaserGate3Done;

        private bool isMachineDone;

        private bool isOffsetVerifyDone;

        private bool isSetResolutionDone;

        private bool isShapeGate1Done;

        private bool isShapeGate2Done;

        private bool isShapeGate3Done;

        private bool isVerticalHomingDone = false;

        #endregion

        #region Constructors

        public InstallationStateViewModel()
        {
            NavigationService.InstallationInfoChangedEventHandler += this.UpdateData;
        }

        #endregion

        #region Properties

        public bool IsBeltBurnishingDone { get => this.isBeltBurnishingDone; set => this.SetProperty(ref this.isBeltBurnishingDone, value); }

        public bool IsCellPositionDone { get => this.isCellPositionDone; set => this.SetProperty(ref this.isCellPositionDone, value); }

        public bool IsGate1Done { get => this.isGate1Done; set => this.SetProperty(ref this.isGate1Done, value); }

        public bool IsGate2Done { get => this.isGate2Done; set => this.SetProperty(ref this.isGate2Done, value); }

        public bool IsGate3Done { get => this.isGate3Done; set => this.SetProperty(ref this.isGate3Done, value); }

        public bool IsHorizontalHomingDone { get => this.isHorizontalHomingDone; set => this.SetProperty(ref this.isHorizontalHomingDone, value); }

        public bool IsLaserGate1Done { get => this.isLaserGate1Done; set => this.SetProperty(ref this.isLaserGate1Done, value); }

        public bool IsLaserGate2Done { get => this.isLaserGate2Done; set => this.SetProperty(ref this.isLaserGate2Done, value); }

        public bool IsLaserGate3Done { get => this.isLaserGate3Done; set => this.SetProperty(ref this.isLaserGate3Done, value); }

        public bool IsMachineDone { get => this.isMachineDone; set => this.SetProperty(ref this.isMachineDone, value); }

        public bool IsOffsetVerifyDone { get => this.isOffsetVerifyDone; set => this.SetProperty(ref this.isOffsetVerifyDone, value); }

        public bool IsSetResolutionDone { get => this.isSetResolutionDone; set => this.SetProperty(ref this.isSetResolutionDone, value); }

        public bool IsShapeGate1Done { get => this.isShapeGate1Done; set => this.SetProperty(ref this.isShapeGate1Done, value); }

        public bool IsShapeGate2Done { get => this.isShapeGate2Done; set => this.SetProperty(ref this.isShapeGate2Done, value); }

        public bool IsShapeGate3Done { get => this.isShapeGate3Done; set => this.SetProperty(ref this.isShapeGate3Done, value); }

        public bool IsVerticalHomingDone { get => this.isVerticalHomingDone; set => this.SetProperty(ref this.isVerticalHomingDone, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            throw new NotImplementedException();
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
            this.Data = (DataManager)this.Container.Resolve<IDataManager>();
            this.UpdateData();
        }

        public void SubscribeMethodToEvent()
        {
            throw new NotImplementedException();
        }

        public void UnSubscribeMethodFromEvent()
        {
            throw new NotImplementedException();
        }

        private void UpdateData()
        {
            this.IsBeltBurnishingDone = this.Data.InstallationInfo.Belt_Burnishing;
            this.IsGate1Done = this.Data.InstallationInfo.Ok_Gate1;
            this.IsGate2Done = this.Data.InstallationInfo.Ok_Gate2;
            this.IsGate3Done = this.Data.InstallationInfo.Ok_Gate3;
            this.IsHorizontalHomingDone = this.Data.InstallationInfo.Origin_X_Axis;
            this.IsLaserGate1Done = this.Data.InstallationInfo.Ok_Laser1;
            this.IsLaserGate2Done = this.Data.InstallationInfo.Ok_Laser2;
            this.IsLaserGate3Done = this.Data.InstallationInfo.Ok_Laser3;
            this.IsMachineDone = this.Data.InstallationInfo.Machine_Ok;
            this.IsShapeGate1Done = this.Data.InstallationInfo.Ok_Shape1;
            this.IsShapeGate2Done = this.Data.InstallationInfo.Ok_Shape2;
            this.IsShapeGate3Done = this.Data.InstallationInfo.Ok_Shape3;
            this.IsSetResolutionDone = this.Data.InstallationInfo.Set_Y_Resolution;
            this.IsVerticalHomingDone = this.Data.InstallationInfo.Origin_Y_Axis;
        }

        #endregion
    }
}
