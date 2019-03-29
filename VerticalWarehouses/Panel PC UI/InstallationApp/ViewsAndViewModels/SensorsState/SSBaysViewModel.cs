using Ferretto.VW.Common_Utils.IO;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSBaysViewModel : BindableBase, ISSBaysViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool gateSensorABay1;

        private bool gateSensorABay2;

        private bool gateSensorABay3;

        private bool gateSensorBBay1;

        private bool gateSensorBBay2;

        private bool gateSensorBBay3;

        private bool heightControlCheck1;

        private bool heightControlCheck2;

        private bool heightControlCheck3;

        private InstallationHubClient installationHubClient;

        private IOSensorsStatus ioSensorsStatus;

        private bool luPresentInBay1;

        private bool luPresentInBay2;

        private bool luPresentInBay3;

        private SubscriptionToken updateSensorsStateToken;

        #endregion

        #region Constructors

        public SSBaysViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.ioSensorsStatus = new IOSensorsStatus();
        }

        #endregion

        #region Properties

        public bool GateSensorABay1 { get => this.gateSensorABay1; set => this.SetProperty(ref this.gateSensorABay1, value); }

        public bool GateSensorABay2 { get => this.gateSensorABay1; set => this.SetProperty(ref this.gateSensorABay2, value); }

        public bool GateSensorABay3 { get => this.gateSensorABay1; set => this.SetProperty(ref this.gateSensorABay3, value); }

        public bool GateSensorBBay1 { get => this.gateSensorBBay1; set => this.SetProperty(ref this.gateSensorBBay1, value); }

        public bool GateSensorBBay2 { get => this.gateSensorBBay1; set => this.SetProperty(ref this.gateSensorBBay2, value); }

        public bool GateSensorBBay3 { get => this.gateSensorBBay1; set => this.SetProperty(ref this.gateSensorBBay3, value); }

        public bool HeightControlCheck1 { get => this.heightControlCheck1; set => this.SetProperty(ref this.heightControlCheck1, value); }

        public bool HeightControlCheck2 { get => this.heightControlCheck1; set => this.SetProperty(ref this.heightControlCheck2, value); }

        public bool HeightControlCheck3 { get => this.heightControlCheck1; set => this.SetProperty(ref this.heightControlCheck3, value); }

        public bool LuPresentInBay1 { get => this.luPresentInBay1; set => this.SetProperty(ref this.luPresentInBay1, value); }

        public bool LuPresentInBay2 { get => this.luPresentInBay1; set => this.SetProperty(ref this.luPresentInBay2, value); }

        public bool LuPresentInBay3 { get => this.luPresentInBay1; set => this.SetProperty(ref this.luPresentInBay3, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
        }

        public void SubscribeMethodToEvent()
        {
            this.updateSensorsStateToken = this.eventAggregator.GetEvent<MAS_Event>()
                .Subscribe(
                message => this.UpdateSensorsStates((message.Data as INotificationMessageSensorsChangedData).SensorsStates),
                ThreadOption.PublisherThread,
                false,
                message => message.NotificationType == NotificationType.SensorsChanged);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.updateSensorsStateToken);
        }

        private void UpdateSensorsStates(bool[] message)
        {
            this.ioSensorsStatus.UpdateInputStates(message);

            this.LuPresentInBay1 = this.ioSensorsStatus.LUPresentInBay1;
            this.HeightControlCheck1 = this.ioSensorsStatus.HeightControlCheckBay1;
            this.GateSensorABay1 = this.ioSensorsStatus.GateSensorABay1;
            this.GateSensorBBay1 = this.ioSensorsStatus.GateSensorBBay1;

            this.LuPresentInBay2 = this.ioSensorsStatus.LUPresentInBay2;
            this.HeightControlCheck2 = this.ioSensorsStatus.HeightControlCheckBay2;
            this.GateSensorABay2 = this.ioSensorsStatus.GateSensorABay2;
            this.GateSensorBBay2 = this.ioSensorsStatus.GateSensorBBay2;

            this.LuPresentInBay3 = this.ioSensorsStatus.LUPresentInBay3;
            this.HeightControlCheck3 = this.ioSensorsStatus.HeightControlCheckBay3;
            this.GateSensorABay3 = this.ioSensorsStatus.GateSensorABay3;
            this.GateSensorBBay3 = this.ioSensorsStatus.GateSensorBBay3;

            // TEMP These code lines will be remove
            //this.LuPresentInBay1 = message[0];
            //this.HeightControlCheck1 = message[1];
            //this.GateSensorABay1 = message[2];
            //this.GateSensorBBay1 = message[3];
        }

        #endregion
    }
}
