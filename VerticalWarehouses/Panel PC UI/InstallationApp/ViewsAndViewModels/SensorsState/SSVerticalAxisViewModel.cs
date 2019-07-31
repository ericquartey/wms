using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
//using Ferretto.VW.CommonUtils.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState
{
    public class SSVerticalAxisViewModel : BindableBase, ISSVerticalAxisViewModel
    {
        #region Fields

        private const int INVERTER_INPUTS = 64;

        private const int REMOTEIO_INPUTS = 48;

        private readonly IEventAggregator eventAggregator;

        private readonly IUnityContainer container;

        private bool cradleEngineSelected;

        private bool elevatorEngineSelected;

        private bool emergencyEndRun;

        private bool luPresentiInMachineSide;

        private bool luPresentInOperatorSide;

        private bool[] sensorStatus;

        private readonly IUpdateSensorsMachineService updateSensorsService;

        private SubscriptionToken updateVerticalandCradleSensorsState;

        private bool zeroPawlSensor;

        private bool zeroVerticalSensor;

        #endregion

        #region Constructors

        public SSVerticalAxisViewModel(
            IEventAggregator eventAggregator,
            IUpdateSensorsMachineService updateSensorsService)
        {
            if (eventAggregator == null)
            {
                throw new System.ArgumentNullException(nameof(eventAggregator));
            }

            if (updateSensorsService == null)
            {
                throw new System.ArgumentNullException(nameof(updateSensorsService));
            }

            this.eventAggregator = eventAggregator;
            this.updateSensorsService = updateSensorsService;
            this.NavigationViewModel = null;
            this.sensorStatus = new bool[REMOTEIO_INPUTS + INVERTER_INPUTS];
        }

        #endregion

        #region Properties

        public bool CradleEngineSelected { get => this.cradleEngineSelected; set => this.SetProperty(ref this.cradleEngineSelected, value); }

        public bool ElevatorEngineSelected { get => this.elevatorEngineSelected; set => this.SetProperty(ref this.elevatorEngineSelected, value); }

        public bool EmergencyEndRun { get => this.emergencyEndRun; set => this.SetProperty(ref this.emergencyEndRun, value); }

        public bool LuPresentInMachineSide { get => this.luPresentiInMachineSide; set => this.SetProperty(ref this.luPresentiInMachineSide, value); }

        public bool LuPresentInOperatorSide { get => this.luPresentInOperatorSide; set => this.SetProperty(ref this.luPresentInOperatorSide, value); }

        public BindableBase NavigationViewModel { get; set; }

        public bool[] SensorStatus { get => this.sensorStatus; set => this.SetProperty(ref this.sensorStatus, value); }

        public bool ZeroPawlSensor { get => this.zeroPawlSensor; set => this.SetProperty(ref this.zeroPawlSensor, value); }

        public bool ZeroVerticalSensor { get => this.zeroVerticalSensor; set => this.SetProperty(ref this.zeroVerticalSensor, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public async Task OnEnterViewAsync()
        {
            //this.DisableVerticalandCradleSensorsState();
            this.updateVerticalandCradleSensorsState = this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                message => this.UpdateVerticalandCradleSensorsState(message.Data.SensorsStates),
                ThreadOption.PublisherThread,
                false);

            await this.updateSensorsService.ExecuteAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Unsubscribe(this.updateVerticalandCradleSensorsState);
        }

        //private void DisableVerticalandCradleSensorsState()
        //{
        //    this.EmergencyEndRun = false;
        //    this.ZeroVerticalSensor = false;
        //    this.ElevatorEngineSelected = false;
        //    this.CradleEngineSelected = false;
        //    this.ZeroPawlSensor = false;
        //    this.LuPresentInMachineSide = false;
        //    this.LuPresentInOperatorSide = false;
        //}

        private void UpdateVerticalandCradleSensorsState(bool[] message)
        {
            this.SensorStatus = message;

            //this.ioSensorsStatus.UpdateInputStates(message);

            //this.EmergencyEndRun = this.ioSensorsStatus.EmergencyEndRun;
            //this.ZeroVerticalSensor = this.ioSensorsStatus.ZeroVertical;
            //this.ElevatorEngineSelected = this.ioSensorsStatus.ElevatorMotorSelected;
            //this.CradleEngineSelected = this.ioSensorsStatus.CradleMotorSelected;
            //this.ZeroPawlSensor = this.ioSensorsStatus.ZeroPawl;
            //this.LuPresentInMachineSide = this.ioSensorsStatus.LuPresentiInMachineSide;
            //this.LuPresentInOperatorSide = this.ioSensorsStatus.LuPresentInOperatorSide;
        }

        #endregion
    }
}
