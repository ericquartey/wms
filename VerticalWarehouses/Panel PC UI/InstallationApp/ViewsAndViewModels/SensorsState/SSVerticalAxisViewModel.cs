using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSVerticalAxisViewModel : BindableBase, ISSVerticalAxisViewModel
    {
        #region Fields

        private IUnityContainer container;

        private bool cradleEngineSelected;

        private bool elevatorEngineSelected;

        private bool emergencyEndRun;

        private IEventAggregator eventAggregator;

        private IInstallationService installationService;

        private IOSensorsStatus ioSensorsStatus;

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide;

        private SubscriptionToken updateVerticalandCradleSensorsState;

        private bool zeroPawlSensor;

        private bool zeroVerticalSensor;

        #endregion

        #region Constructors

        public SSVerticalAxisViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.ioSensorsStatus = new IOSensorsStatus();
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public bool CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }

        public bool ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }

        public bool EmergencyEndRun { get => this.emergencyEndRun; set => this.SetProperty(ref this.emergencyEndRun, value); }

        public bool LuPresentiInMachineSide { get => this.luPresentiInMachineSide; set => this.SetProperty(ref this.luPresentiInMachineSide, value); }

        public bool LuPresentInOperatorSide { get => this.luPresentInOperatorSide; set => this.SetProperty(ref this.luPresentInOperatorSide, value); }

        public BindableBase NavigationViewModel { get; set; }

        public bool ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        public bool ZeroVerticalSensor { get => this.zeroVerticalSensor; set => this.SetProperty(ref this.zeroVerticalSensor, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.installationService = this.container.Resolve<IInstallationService>();
        }

        public async Task OnEnterViewAsync()
        {
            this.updateVerticalandCradleSensorsState = this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                message => this.UpdateVerticalandCradleSensorsState(message.Data.SensorsStates),
                ThreadOption.PublisherThread,
                false);

            this.installationService.ExecuteSensorsChangedAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateVerticalandCradleSensorsState);
        }

        private void UpdateVerticalandCradleSensorsState(bool[] message)
        {
            this.ioSensorsStatus.UpdateInputStates(message);

            this.EmergencyEndRun = this.ioSensorsStatus.EmergencyEndRun;
            this.ZeroVerticalSensor = this.ioSensorsStatus.ZeroVertical;
            this.ElevatorEngineSelected = this.ioSensorsStatus.ElevatorMotorSelected;
            this.CradleEngineSelected = this.ioSensorsStatus.CradleMotorSelected;
            this.ZeroPawlSensor = this.ioSensorsStatus.ZeroPawl;
            this.LuPresentiInMachineSide = this.ioSensorsStatus.LuPresentiInMachineSide;
            this.LuPresentInOperatorSide = this.ioSensorsStatus.LuPresentInOperatorSide;
        }

        #endregion
    }
}
