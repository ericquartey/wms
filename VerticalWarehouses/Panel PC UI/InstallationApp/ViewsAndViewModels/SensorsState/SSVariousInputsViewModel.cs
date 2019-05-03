using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVariousInputsViewModel : BindableBase, ISSVariousInputsViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private bool antiIntrusionShutterBay1;

        private bool antiIntrusionShutterBay2;

        private bool antiIntrusionShutterBay3;

        private IUnityContainer container;

        private bool cradleEngineSelected;

        private bool elevatorEngineSelected;

        private IOSensorsStatus ioSensorsStatus;

        private bool microCarterLeftSideBay1;

        private bool microCarterLeftSideBay2;

        private bool microCarterLeftSideBay3;

        private bool microCarterRightSideBay1;

        private bool microCarterRightSideBay2;

        private bool microCarterRightSideBay3;

        private bool mushroomHeadButtonBay1;

        private bool mushroomHeadButtonBay2;

        private bool mushroomHeadButtonBay3;

        private bool securityFunctionActive;

        private SubscriptionToken updateVariousInputsSensorsState;

        #endregion

        #region Constructors

        public SSVariousInputsViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.ioSensorsStatus = new IOSensorsStatus();
        }

        #endregion

        #region Properties

        public bool AntiIntrusionShutterBay1 { get => this.antiIntrusionShutterBay1; set => this.SetProperty(ref this.antiIntrusionShutterBay1, value); }

        public bool AntiIntrusionShutterBay2 { get => this.antiIntrusionShutterBay2; set => this.SetProperty(ref this.antiIntrusionShutterBay2, value); }

        public bool AntiIntrusionShutterBay3 { get => this.antiIntrusionShutterBay3; set => this.SetProperty(ref this.antiIntrusionShutterBay3, value); }

        public bool CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }

        public bool ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }

        public bool MicroCarterLeftSideBay1 { get => this.microCarterLeftSideBay1; set => this.SetProperty(ref this.microCarterLeftSideBay1, value); }

        public bool MicroCarterLeftSideBay2 { get => this.microCarterLeftSideBay2; set => this.SetProperty(ref this.microCarterLeftSideBay2, value); }

        public bool MicroCarterLeftSideBay3 { get => this.microCarterLeftSideBay3; set => this.SetProperty(ref this.microCarterLeftSideBay3, value); }

        public bool MicroCarterRightSideBay1 { get => this.microCarterRightSideBay1; set => this.SetProperty(ref this.microCarterRightSideBay1, value); }

        public bool MicroCarterRightSideBay2 { get => this.microCarterRightSideBay2; set => this.SetProperty(ref this.microCarterRightSideBay2, value); }

        public bool MicroCarterRightSideBay3 { get => this.microCarterRightSideBay3; set => this.SetProperty(ref this.microCarterRightSideBay3, value); }

        public bool MushroomHeadButtonBay1 { get => this.mushroomHeadButtonBay1; set => this.SetProperty(ref this.mushroomHeadButtonBay1, value); }

        public bool MushroomHeadButtonBay2 { get => this.mushroomHeadButtonBay2; set => this.SetProperty(ref this.mushroomHeadButtonBay2, value); }

        public bool MushroomHeadButtonBay3 { get => this.mushroomHeadButtonBay3; set => this.SetProperty(ref this.mushroomHeadButtonBay3, value); }

        public bool SecurityFunctionActive { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public async Task OnEnterViewAsync()
        {
            this.updateVariousInputsSensorsState = this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                message => this.UpdateVariousInputsSensorsState(message.Data.SensorsStates),
                ThreadOption.PublisherThread,
                false);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Unsubscribe(this.updateVariousInputsSensorsState);
        }

        private void UpdateVariousInputsSensorsState(bool[] message)
        {
            this.ioSensorsStatus.UpdateInputStates(message);

            this.SecurityFunctionActive = this.ioSensorsStatus.SecurityFunctionActive;

            this.MushroomHeadButtonBay1 = this.ioSensorsStatus.MushroomHeadButtonBay1;
            this.MicroCarterLeftSideBay1 = this.ioSensorsStatus.MicroCarterLeftSideBay1;
            this.MicroCarterRightSideBay1 = this.ioSensorsStatus.MicroCarterRightSideBay1;
            this.AntiIntrusionShutterBay1 = this.ioSensorsStatus.AntiIntrusionShutterBay1;

            this.MushroomHeadButtonBay2 = this.ioSensorsStatus.MushroomHeadButtonBay2;
            this.MicroCarterLeftSideBay2 = this.ioSensorsStatus.MicroCarterLeftSideBay2;
            this.MicroCarterRightSideBay2 = this.ioSensorsStatus.MicroCarterRightSideBay2;
            this.AntiIntrusionShutterBay2 = this.ioSensorsStatus.AntiIntrusionShutterBay2;

            this.MushroomHeadButtonBay3 = this.ioSensorsStatus.MushroomHeadButtonBay3;
            this.MicroCarterLeftSideBay3 = this.ioSensorsStatus.MicroCarterLeftSideBay3;
            this.MicroCarterRightSideBay3 = this.ioSensorsStatus.MicroCarterRightSideBay3;
            this.AntiIntrusionShutterBay3 = this.ioSensorsStatus.AntiIntrusionShutterBay3;
        }

        #endregion
    }
}
