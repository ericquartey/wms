using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState
{
    public class SSVariousInputsViewModel : BindableBase, ISSVariousInputsViewModel
    {
        #region Fields

        private const int INVERTER_INPUTS = 64;

        private const int REMOTEIO_INPUTS = 16;

        private readonly IUnityContainer container;

        private readonly IEventAggregator eventAggregator;

        private readonly IUpdateSensorsMachineService updateSensorsService;

        private bool securityFunctionActive;

        private bool[] sensorStatus;

        private SubscriptionToken updateVariousInputsSensorsState;

        #endregion

        #region Constructors

        public SSVariousInputsViewModel(
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
            this.NavigationViewModel = null;
            this.updateSensorsService = updateSensorsService;
            this.sensorStatus = new bool[REMOTEIO_INPUTS * 3 + INVERTER_INPUTS];
        }

        #endregion

        #region Properties

        public BindableBase NavigationViewModel { get; set; }

        public bool SecurityFunctionActive { get => this.securityFunctionActive; set => this.SetProperty(ref this.securityFunctionActive, value); }

        public bool[] SensorStatus { get => this.sensorStatus; set => this.SetProperty(ref this.sensorStatus, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            this.UnSubscribeMethodFromEvent();
        }

        public async Task OnEnterViewAsync()
        {
            this.updateVariousInputsSensorsState = this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                .Subscribe(
                    message => this.UpdateVariousInputsSensorsState(message.Data.SensorsStates),
                    ThreadOption.PublisherThread,
                    false);

            await this.updateSensorsService.ExecuteAsync();
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<NotificationEventUI<SensorsChangedMessageData>>().Unsubscribe(this.updateVariousInputsSensorsState);
        }

        private void UpdateVariousInputsSensorsState(bool[] message)
        {
            this.SensorStatus = message;
        }

        #endregion
    }
}
