using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState
{
    public class SSBaysViewModel : BindableBase, ISSBaysViewModel
    {
        #region Fields

        private const int INVERTER_INPUTS = 64;

        private const int REMOTEIO_INPUTS = 48;

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool[] sensorStatus;

        private bool shutterSensorABay1;

        private bool shutterSensorABay2;

        private bool shutterSensorABay3;

        private bool shutterSensorBBay1;

        private bool shutterSensorBBay2;

        private bool shutterSensorBBay3;

        private IUpdateSensorsService updateSensorsService;

        private SubscriptionToken updateSensorsStateToken;

        #endregion

        #region Constructors

        public SSBaysViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.sensorStatus = new bool[REMOTEIO_INPUTS + INVERTER_INPUTS];
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public bool[] SensorStatus { get => this.sensorStatus; set => this.SetProperty(ref this.sensorStatus, value); }

        public bool ShutterSensorABay1 { get => this.shutterSensorABay1; set => this.SetProperty(ref this.shutterSensorABay1, value); }

        public bool ShutterSensorABay2 { get => this.shutterSensorABay2; set => this.SetProperty(ref this.shutterSensorABay2, value); }

        public bool ShutterSensorABay3 { get => this.shutterSensorABay3; set => this.SetProperty(ref this.shutterSensorABay3, value); }

        public bool ShutterSensorBBay1 { get => this.shutterSensorBBay1; set => this.SetProperty(ref this.shutterSensorBBay1, value); }

        public bool ShutterSensorBBay2 { get => this.shutterSensorBBay2; set => this.SetProperty(ref this.shutterSensorBBay2, value); }

        public bool ShutterSensorBBay3 { get => this.shutterSensorBBay3; set => this.SetProperty(ref this.shutterSensorBBay3, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.updateSensorsService = this.container.Resolve<IUpdateSensorsService>();
        }

        public async Task OnEnterViewAsync()
        {
            //this.DisableSensorsState();
            this.updateSensorsStateToken = this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                 .Subscribe(
                 message => this.UpdateSensorsStates(message.Data.SensorsStates),
                 ThreadOption.PublisherThread,
                 false);

            await this.updateSensorsService.ExecuteAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Unsubscribe(this.updateSensorsStateToken);
        }

        //private void DisableSensorsState()
        //{
        //    this.LuPresentInBay1 = false;
        //    this.HeightControlCheckBay1 = false;
        //    this.ShutterSensorABay1 = false;
        //    this.ShutterSensorBBay1 = false;

        //    this.LuPresentInBay2 = false;
        //    this.HeightControlCheckBay2 = false;
        //    this.ShutterSensorABay2 = false;
        //    this.ShutterSensorBBay2 = false;

        //    this.LuPresentInBay3 = false;
        //    this.HeightControlCheckBay3 = false;
        //    this.ShutterSensorABay3 = false;
        //    this.ShutterSensorBBay3 = false;
        //}

        private void UpdateSensorsStates(bool[] message)
        {
            this.SensorStatus = message;
        }

        #endregion
    }
}
