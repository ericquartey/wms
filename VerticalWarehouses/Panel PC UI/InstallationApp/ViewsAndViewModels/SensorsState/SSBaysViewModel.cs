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

        private const int REMOTEIO_INPUTS = 16;

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private bool[] sensorStatus;

        private IUpdateSensorsService updateSensorsService;

        private SubscriptionToken updateSensorsStateToken;

        #endregion

        #region Constructors

        public SSBaysViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            this.sensorStatus = new bool[REMOTEIO_INPUTS * 3 + INVERTER_INPUTS];
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public bool[] SensorStatus { get => this.sensorStatus; set => this.SetProperty(ref this.sensorStatus, value); }

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
            this.SensorStatus = message;
        }

        #endregion
    }
}
