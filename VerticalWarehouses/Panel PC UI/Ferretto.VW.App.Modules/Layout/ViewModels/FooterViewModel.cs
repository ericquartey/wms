using System;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    public class FooterViewModel : BasePresentationViewModel
    {
        #region Fields

        private readonly IMachineModeService machineModeService;

        private bool isEnabled;

        private string notificationMessage;

        private NotificationSeverity notificationSeverity;

        private SubscriptionToken subscriptionToken;

        #endregion

        #region Constructors

        public FooterViewModel(IMachineModeService machineModeService)
        {
            if (machineModeService is null)
            {
                throw new ArgumentNullException(nameof(machineModeService));
            }

            this.machineModeService = machineModeService;

            this.subscriptionToken = machineModeService.MachineModeChangedEvent
               .Subscribe(
                   this.OnMachineModeChanged,
                   ThreadOption.UIThread,
                   false);

            this.UpdateIsEnabled(this.machineModeService.MachineMode, this.machineModeService.MachinePower);
        }

        #endregion

        #region Properties

        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }

        public string NotificationMessage
        {
            get => this.notificationMessage;
            set => this.SetProperty(ref this.notificationMessage, value);
        }

        public NotificationSeverity NotificationSeverity
        {
            get => this.notificationSeverity;
            set => this.SetProperty(ref this.notificationSeverity, value);
        }

        #endregion

        #region Methods

        public override void InitializeData()
        {
            base.InitializeData();
            this.States.Add(this.GetInstance(nameof(PresentationBack)));
        }

        public override void UpdateChanges(PresentationChangedMessage message)
        {
            base.UpdateChanges(message);

            if (message.NotificationSeverity == NotificationSeverity.NotSpecified)
            {
                return;
            }

            if (message.NotificationSeverity == NotificationSeverity.Clear)
            {
                this.NotificationMessage = null;
                return;
            }

            this.NotificationSeverity = message.NotificationSeverity;

            if (message.Exception != null)
            {
                if (message.Exception is SwaggerException<ProblemDetails> swaggerException)
                {
                    this.NotificationMessage =
                        swaggerException.Result.Title +
                        System.Environment.NewLine +
                        swaggerException.Result.Detail;
                }
                else if (message.Exception is SwaggerException)
                {
                    this.NotificationMessage = Resources.VWApp.ErrorCommunicatingWithServices;
                }
                else
                {
                    this.NotificationMessage = message.Exception.Message;
                }
            }
            else
            {
                this.NotificationMessage = message.NotificationMessage;
            }
        }

        public override void UpdatePresentation(PresentationMode mode)
        {
            if (mode == PresentationMode.None ||
                this.CurrentPresentation == mode)
            {
                return;
            }

            this.CurrentPresentation = mode;
            switch (mode)
            {
                case PresentationMode.Login:
                    this.Show(PresentationTypes.None, false);
                    break;

                case PresentationMode.Installer:
                    this.Show(PresentationTypes.None, false);
                    break;

                case PresentationMode.Operator:
                    this.Show(PresentationTypes.None, false);
                    break;

                case PresentationMode.Help:
                    this.Show(PresentationTypes.None, false);
                    this.Show(PresentationTypes.Back, true);
                    break;
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            if (this.subscriptionToken != null)
            {
                this.machineModeService.MachineModeChangedEvent.Unsubscribe(this.subscriptionToken);

                this.subscriptionToken = null;
            }
        }

        private void OnMachineModeChanged(MachineModeChangedEventArgs e)
        {
            this.UpdateIsEnabled(e.MachineMode, e.MachinePower);
        }

        private void UpdateIsEnabled(MachineMode machineMode, MachinePowerState machinePower)
        {
            this.IsEnabled = machinePower != MachinePowerState.Unpowered;
        }

        #endregion
    }
}
