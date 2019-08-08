using Ferretto.VW.App.Controls;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class BaysSensorsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly ISensorsMachineService sernsorsMachineService;

        private bool[] sensorsStates;

        private SubscriptionToken updateSensorsStateToken;

        #endregion

        #region Constructors

        public BaysSensorsViewModel(ISensorsMachineService sensorsMachineService)
            : base(Services.PresentationMode.Installator)
        {
            if (this.sernsorsMachineService == null)
            {
                throw new System.ArgumentNullException(nameof(this.sernsorsMachineService));
            }

            this.sensorsMachineService = sensorsMachineService;
        }

        #endregion

        #region Properties

        public bool[] SensorsStates
        {
            get => this.sensorsStates;
            set => this.SetProperty(ref this.sensorsStates, value);
        }

        #endregion

        #region Methods

        public override async void OnNavigated()
        {
            this.updateSensorsStateToken = this.EventAggregator
              .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
              .Subscribe(
                  message => this.SensorsStates = message?.Data?.SensorsStates,
                  ThreadOption.PublisherThread,
                  false);

            try
            {
                await this.sensorsMachineService.ForceRefresh();
            }
            catch (System.Exception ex)
            {
                this.ShowError(ex.Message);
            }

            base.OnNavigated();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.updateSensorsStateToken != null)
            {
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SensorsChangedMessageData>>()
                    .Unsubscribe(this.updateSensorsStateToken);

                this.updateSensorsStateToken = null;
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
