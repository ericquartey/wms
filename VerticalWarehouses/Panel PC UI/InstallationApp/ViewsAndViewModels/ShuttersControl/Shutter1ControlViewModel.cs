using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class Shutter1ControlViewModel : BindableBase, IShutter1ControlViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private string completedCycles;

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private SubscriptionToken receivedActionUpdateToken;

        private string requestedCycles;

        private BindableBase sensorRegion;

        #endregion

        #region Constructors

        public Shutter1ControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string CompletedCycles { get => this.completedCycles; set => this.SetProperty(ref this.completedCycles, value); }

        public bool IsStartButtonActive { get => this.isStartButtonActive; set => this.SetProperty(ref this.isStartButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public string RequestedCycles { get => this.requestedCycles; set => this.SetProperty(ref this.requestedCycles, value); }

        public BindableBase SensorRegion { get => this.sensorRegion; set => this.SetProperty(ref this.sensorRegion, value); }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void SubscribeMethodToEvent()
        {
            this.receivedActionUpdateToken = this.eventAggregator.GetEvent<MAS_Event>().Subscribe(
                msg => this.UpdateCompletedCycles(msg.Data),
                ThreadOption.PublisherThread,
                false,
                message =>
                message.NotificationType == NotificationType.CurrentActionStatus &&
                message.ActionType == ActionType.ShutterPositioning &&
                message.ActionStatus == ActionStatus.Executing);
        }

        public void UnSubscribeMethodFromEvent()
        {
            this.eventAggregator.GetEvent<MAS_Event>().Unsubscribe(this.receivedActionUpdateToken);
        }

        private void UpdateCompletedCycles(INotificationMessageData data)
        {
            if (data is INotificationActionUpdatedMessageData parsedData)
            {
                this.CompletedCycles = parsedData.CurrentShutterPosition.ToString();

                if (int.TryParse(this.RequestedCycles, out var value) && value == parsedData.CurrentShutterPosition)
                {
                    this.IsStartButtonActive = true;
                    this.IsStopButtonActive = false;
                }
                this.CompletedCycles = parsedData.CurrentShutterPosition.ToString();
            }
        }

        #endregion
    }
}
