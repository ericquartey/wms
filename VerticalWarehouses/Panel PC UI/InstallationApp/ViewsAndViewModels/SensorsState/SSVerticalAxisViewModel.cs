using System.Threading.Tasks;
using Ferretto.VW.App.Installation.Interfaces;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.ViewsAndViewModels.SensorsState
{
    public class SSVerticalAxisViewModel : BindableBase, ISSVerticalAxisViewModel
    {
        #region Fields

        private const int INVERTER_INPUTS = 64;

        private const int REMOTEIO_INPUTS = 16;

        private readonly IEventAggregator eventAggregator;

        private readonly IUpdateSensorsMachineService updateSensorsService;

        private bool[] sensorStatus;

        private SubscriptionToken updateVerticalandCradleSensorsState;

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

        public async Task OnEnterViewAsync()
        {
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

        private void UpdateVerticalandCradleSensorsState(bool[] message)
        {
            this.SensorStatus = message;
        }

        #endregion
    }
}
