using System;
using System.Configuration;
using System.Net.Http;
using System.Windows.Input;
using Ferretto.VW.Common_Utils.Messages.MAStoUIMessages.Enumerations;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.InstallationApp.Interfaces;
using Ferretto.VW.InstallationApp.Resources;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.InstallationApp
{
    public class Shutter1ControlViewModel : BindableBase, IShutter1ControlViewModel
    {
        #region Fields

        private readonly string installationController = ConfigurationManager.AppSettings.Get("InstallationController");

        private readonly string startShutter1Controller = ConfigurationManager.AppSettings.Get("InstallationStartShutter1");

        private readonly string stopShutter1Controller = ConfigurationManager.AppSettings.Get("InstallationStopShutter1");

        private int bayID;

        private int bayType;

        private string completedCycles;

        private IUnityContainer container;

        private string delayBetweenCycles;

        private IEventAggregator eventAggregator;

        private string getIntegerValuesController = ConfigurationManager.AppSettings.Get("InstallationGetIntegerConfigurationValues");

        private bool isStartButtonActive = true;

        private bool isStopButtonActive;

        private SubscriptionToken receivedActionUpdateToken;

        private string requestedCycles;

        private BindableBase sensorRegion;

        private ICommand startButtonCommand;

        private ICommand stopButtonCommand;

        #endregion

        #region Constructors

        public Shutter1ControlViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public string CompletedCycles { get => this.completedCycles; set => this.SetProperty(ref this.completedCycles, value); }

        public string DelayBetweenCycles { get => this.delayBetweenCycles; set => this.SetProperty(ref this.delayBetweenCycles, value); }

        public bool IsStartButtonActive { get => this.isStartButtonActive; set => this.SetProperty(ref this.isStartButtonActive, value); }

        public bool IsStopButtonActive { get => this.isStopButtonActive; set => this.SetProperty(ref this.isStopButtonActive, value); }

        public string RequestedCycles { get => this.requestedCycles; set => this.SetProperty(ref this.requestedCycles, value); }

        public BindableBase SensorRegion { get => this.sensorRegion; set => this.SetProperty(ref this.sensorRegion, value); }

        public ICommand StartButtonCommand => this.startButtonCommand ?? (this.startButtonCommand = new DelegateCommand(this.ExecuteStartButtonCommand));

        public ICommand StopButtonCommand => this.stopButtonCommand ?? (this.stopButtonCommand = new DelegateCommand(() => this.ExecuteStopButtonCommand()));

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

        public async void OnEnterView()
        {
            if (this.bayType == null)
            {
                var client = new HttpClient();
                var response = await client.GetAsync(new Uri(this.installationController + this.getIntegerValuesController + this.bayID + "BayType"));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    this.bayType = response.Content.ReadAsAsync<int>().Result;
                }
            }
            if (this.bayType == 1)
            {
                this.sensorRegion = (CustomShutterControlSensorsThreePositionsViewModel)this.container.Resolve<ICustomShutterControlSensorsThreePositionsViewModel>();
            }
            else
            {
                this.sensorRegion = (CustomShutterControlSensorsTwoPositionsViewModel)this.container.Resolve<ICustomShutterControlSensorsTwoPositionsViewModel>();
            }

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

        private async void ExecuteStartButtonCommand()
        {
            try
            {
                var client = new HttpClient();
                await client.GetStringAsync(new Uri(this.installationController + this.startShutter1Controller + $"/{this.delayBetweenCycles}/{this.requestedCycles}"));
                this.IsStartButtonActive = false;
                this.IsStopButtonActive = true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void ExecuteStopButtonCommand()
        {
            try
            {
                var client = new HttpClient();
                await client.GetStringAsync(new Uri(this.installationController + this.stopShutter1Controller));
                this.IsStartButtonActive = true;
                this.IsStopButtonActive = false;
            }
            catch (Exception)
            {
                throw;
            }
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
