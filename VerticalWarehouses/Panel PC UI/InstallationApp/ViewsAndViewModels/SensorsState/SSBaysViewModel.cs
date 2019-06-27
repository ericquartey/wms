using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_AutomationService.Contracts;
using Ferretto.VW.MAS_Utils.Events;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.InstallationApp
{
    public class SSBaysViewModel : BindableBase, ISSBaysViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool heightControlCheckBay1;

        private bool heightControlCheckBay2;

        private bool heightControlCheckBay3;

        private IOSensorsStatus ioSensorsStatus;

        private bool luPresentInBay1;

        private bool luPresentInBay2;

        private bool luPresentInBay3;

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
            this.ioSensorsStatus = new IOSensorsStatus();
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public bool HeightControlCheckBay1 { get => this.heightControlCheckBay1; set => this.SetProperty(ref this.heightControlCheckBay1, value); }

        public bool HeightControlCheckBay2 { get => this.heightControlCheckBay2; set => this.SetProperty(ref this.heightControlCheckBay2, value); }

        public bool HeightControlCheckBay3 { get => this.heightControlCheckBay3; set => this.SetProperty(ref this.heightControlCheckBay3, value); }

        public bool LuPresentInBay1 { get => this.luPresentInBay1; set => this.SetProperty(ref this.luPresentInBay1, value); }

        public bool LuPresentInBay2 { get => this.luPresentInBay2; set => this.SetProperty(ref this.luPresentInBay2, value); }

        public bool LuPresentInBay3 { get => this.luPresentInBay3; set => this.SetProperty(ref this.luPresentInBay3, value); }

        public BindableBase NavigationViewModel { get; set; }

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

        private void UpdateSensorsStates(bool[] message)
        {
            this.ioSensorsStatus.UpdateInputStates(message);

            this.LuPresentInBay1 = this.ioSensorsStatus.LUPresentInBay1;
            this.HeightControlCheckBay1 = this.ioSensorsStatus.HeightControlCheckBay1;
            this.ShutterSensorABay1 = this.ioSensorsStatus.ShutterSensorABay1;
            this.ShutterSensorBBay1 = this.ioSensorsStatus.ShutterSensorBBay1;

            this.LuPresentInBay2 = this.ioSensorsStatus.LUPresentInBay2;
            this.HeightControlCheckBay2 = this.ioSensorsStatus.HeightControlCheckBay2;
            this.ShutterSensorABay2 = this.ioSensorsStatus.ShutterSensorABay2;
            this.ShutterSensorBBay2 = this.ioSensorsStatus.ShutterSensorBBay2;

            this.LuPresentInBay3 = this.ioSensorsStatus.LUPresentInBay3;
            this.HeightControlCheckBay3 = this.ioSensorsStatus.HeightControlCheckBay3;
            this.ShutterSensorABay3 = this.ioSensorsStatus.ShutterSensorABay3;
            this.ShutterSensorBBay3 = this.ioSensorsStatus.ShutterSensorBBay3;
        }

        #endregion
    }
}
