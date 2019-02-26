using Prism.Mvvm;
using Ferretto.VW.Utils.Source;
using System;
using Microsoft.Practices.Unity;
using Prism.Events;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.Resources.Enumerables;

namespace Ferretto.VW.InstallationApp
{
    public class InstallationStateViewModel : BindableBase, IViewModel, IInstallationStateViewModel
    {
        #region Fields

        public IUnityContainer Container;

        public DataManager Data;

        private IEventAggregator eventAggregator;

        private bool isBeltBurnishingDone;

        private bool isCellPositionDone;

        private bool isHorizontalHomingDone;

        private bool isLaserShutter1Done;

        private bool isLaserShutter2Done;

        private bool isLaserShutter3Done;

        private bool isMachineDone;

        private bool isOffsetVerifyDone;

        private bool isSetResolutionDone;

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
            this.eventAggregator.GetEvent<InstallationApp_Event>().Subscribe((message) => { this.UpdateData(); },
                ThreadOption.PublisherThread,
                false,
                message => message.Type == InstallationApp_EventMessageType.InstallationInfoChanged);
        }

        #endregion

        #region Properties

        public bool IsBeltBurnishingDone { get => this.isBeltBurnishingDone; set => this.SetProperty(ref this.isBeltBurnishingDone, value); }

        public bool IsCellPositionDone { get => this.isCellPositionDone; set => this.SetProperty(ref this.isCellPositionDone, value); }

        public bool IsHorizontalHomingDone { get => this.isHorizontalHomingDone; set => this.SetProperty(ref this.isHorizontalHomingDone, value); }

        public bool IsLaserShutter1Done { get => this.isLaserShutter1Done; set => this.SetProperty(ref this.isLaserShutter1Done, value); }

        public bool IsLaserShutter2Done { get => this.isLaserShutter2Done; set => this.SetProperty(ref this.isLaserShutter2Done, value); }

        public bool IsLaserShutter3Done { get => this.isLaserShutter3Done; set => this.SetProperty(ref this.isLaserShutter3Done, value); }

        public bool IsMachineDone { get => this.isMachineDone; set => this.SetProperty(ref this.isMachineDone, value); }

        public bool IsOffsetVerifyDone { get => this.isOffsetVerifyDone; set => this.SetProperty(ref this.isOffsetVerifyDone, value); }

        public bool IsSetResolutionDone { get => this.isSetResolutionDone; set => this.SetProperty(ref this.isSetResolutionDone, value); }

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
            // TODO
        }

        public void InitializeViewModel(IUnityContainer _container)
        {
            this.Container = _container;
            this.Data = (DataManager)this.Container.Resolve<IDataManager>();
            this.UpdateData();
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        private void UpdateData()
        {
            this.IsBeltBurnishingDone = this.Data.InstallationInfo.Belt_Burnishing;
            this.IsShutter1InstallationProcedureDone = this.Data.InstallationInfo.Ok_Gate1;
            this.IsShutter2InstallationProcedureDone = this.Data.InstallationInfo.Ok_Gate2;
            this.IsShutter3InstallationProcedureDone = this.Data.InstallationInfo.Ok_Gate3;
            this.IsHorizontalHomingDone = this.Data.InstallationInfo.Origin_X_Axis;
            this.IsLaserShutter1Done = this.Data.InstallationInfo.Ok_Laser1;
            this.IsLaserShutter2Done = this.Data.InstallationInfo.Ok_Laser2;
            this.IsLaserShutter3Done = this.Data.InstallationInfo.Ok_Laser3;
            this.IsMachineDone = this.Data.InstallationInfo.Machine_Ok;
            this.IsShapeShutter1Done = this.Data.InstallationInfo.Ok_Shape1;
            this.IsShapeShutter2Done = this.Data.InstallationInfo.Ok_Shape2;
            this.IsShapeShutter3Done = this.Data.InstallationInfo.Ok_Shape3;
            this.IsSetResolutionDone = this.Data.InstallationInfo.Set_Y_Resolution;
            this.IsVerticalHomingDone = this.Data.InstallationInfo.Origin_Y_Axis;
        }

        #endregion
    }
}
