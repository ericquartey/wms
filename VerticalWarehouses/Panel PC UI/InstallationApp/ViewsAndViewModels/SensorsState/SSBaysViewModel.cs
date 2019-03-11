using Ferretto.VW.InstallationApp.Resources;
using Ferretto.VW.InstallationApp.ServiceUtilities;
using Ferretto.VW.InstallationApp.ServiceUtilities.Interfaces;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class SSBaysViewModel : BindableBase, IViewModel, ISSBaysViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool gateSensorABay1;

        private bool gateSensorBBay1;

        private bool heightControlCheck1;

        private InstallationHubClient installationHubClient;

        private bool luPresentInBay1;

        #endregion

        #region Constructors

        public SSBaysViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public bool GateSensorABay1 { get => this.gateSensorABay1; set => this.SetProperty(ref this.gateSensorABay1, value); }

        public bool GateSensorBBay1 { get => this.gateSensorBBay1; set => this.SetProperty(ref this.gateSensorBBay1, value); }

        public bool HeightControlCheck1 { get => this.heightControlCheck1; set => this.SetProperty(ref this.heightControlCheck1, value); }

        public bool LuPresentInBay1 { get => this.luPresentInBay1; set => this.SetProperty(ref this.luPresentInBay1, value); }

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
            this.installationHubClient = (InstallationHubClient)this.container.Resolve<IContainerInstallationHubClient>();
            this.installationHubClient.SensorsChanged += this.UpdateSensorsStates;
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<InstallationApp_Event>().Unsubscribe((message) => { this.SubscribeMethodToEvent(); });
            this.eventAggregator.GetEvent<InstallationApp_Event>().Unsubscribe((message) => { this.UnSubscribeMethodFromEvent(); });
        }

        private void UpdateSensorsStates(object sender, bool[] message)
        {
            this.LuPresentInBay1 = message[0];
            this.HeightControlCheck1 = message[1];
            this.GateSensorABay1 = message[2];
            this.GateSensorBBay1 = message[3];
        }

        #endregion
    }
}
