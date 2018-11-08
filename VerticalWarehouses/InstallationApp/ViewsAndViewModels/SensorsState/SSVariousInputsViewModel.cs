using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp.ViewsAndViewModels.SensorsState
{
    internal class SSVariousInputsViewModel : BindableBase
    {
        #region Fields

        private bool antiIntrusionGate;
        private bool cradleEngineSelected = true;
        private bool elevatorEngineSelected;
        private bool microCarterLeftSide = true;
        private bool microCarterRightSide;
        private bool mushroomHeadButton;
        private bool securityFunctionActive = true;

        #endregion Fields

        #region Properties

        public System.Boolean AntiIntrusionGate { get => this.antiIntrusionGate; set => this.SetProperty(ref this.antiIntrusionGate, value); }
        public System.Boolean CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }
        public System.Boolean ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }
        public System.Boolean MicroCarterLeftSide { get => this.microCarterLeftSide; set => this.SetProperty(ref this.microCarterLeftSide, value); }
        public System.Boolean MicroCarterRightSide { get => this.microCarterRightSide; set => this.SetProperty(ref this.microCarterRightSide, value); }
        public System.Boolean MushroomHeadButton { get => this.mushroomHeadButton; set => this.SetProperty(ref this.mushroomHeadButton, value); }
        public System.Boolean SecurityFunctionActive { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        #endregion Properties
    }
}
