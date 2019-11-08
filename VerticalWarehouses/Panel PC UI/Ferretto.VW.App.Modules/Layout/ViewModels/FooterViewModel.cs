using System.Linq;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    internal sealed class FooterViewModel : BasePresentationViewModel
    {
        #region Fields

        private string notificationMessage;

        private NotificationSeverity notificationSeverity;

        #endregion

        #region Constructors

        public FooterViewModel()
            : base()
        {
            var notificationEvent = this.EventAggregator.GetEvent<PresentationNotificationPubSubEvent>();

            notificationEvent.Subscribe(
                notificationMessage => this.NotificationChanged(notificationMessage),
                ThreadOption.UIThread,
                true);
        }

        #endregion

        #region Properties

        public bool IsEnabled => true;

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

            var prev = this.GetInstance<PresentationNavigationStep>();
            prev.Type = PresentationTypes.Prev;
            this.States.Add(prev);

            var next = this.GetInstance<PresentationNavigationStep>();
            next.Type = PresentationTypes.Next;
            this.States.Add(next);

            this.States.Add(this.GetInstance<PresentationAbort>());
            this.States.Add(this.GetInstance<PresentationBack>());
        }

        public void NotificationChanged(PresentationNotificationMessage message)
        {
            if (message is null)
            {
                return;
            }

            if (message.ClearMessage)
            {
                this.NotificationMessage = null;
                return;
            }

            if (message.NotificationSeverity == NotificationSeverity.NotSpecified)
            {
                return;
            }

            this.NotificationSeverity = message.NotificationSeverity;

            if (message.Exception != null)
            {
                if (message.Exception is SwaggerException<ProblemDetails> swaggerException)
                {
                    if (swaggerException.Result is null)
                    {
                        this.NotificationMessage = swaggerException.Message;
                    }
                    else
                    {
                        if (swaggerException.Result.Detail is null)
                        {
                            this.NotificationMessage =
                            swaggerException.Result.Title +
                            System.Environment.NewLine +
                            ((!(swaggerException.Result is null)
                             &&
                             swaggerException.Result.AdditionalProperties.Any()) ? swaggerException.Result.AdditionalProperties.First().Value : string.Empty);
                        }
                        else
                        {
                            this.NotificationMessage =
                            swaggerException.Result.Title +
                            System.Environment.NewLine +
                            swaggerException.Result.Detail?.Split('\n', '\r').FirstOrDefault();
                        }
                    }
                }
                else if (message.Exception is SwaggerException)
                {
                    var notificationMessage = Resources.VWApp.ErrorCommunicatingWithServices;

                    if (message.Exception.InnerException != null)
                    {
                        notificationMessage +=
                            System.Environment.NewLine +
                            message.Exception.InnerException.Message;
                    }
                    else
                    {
                        notificationMessage +=
                           System.Environment.NewLine +
                           message.Exception.Message.Split('\n', '\r').FirstOrDefault();
                    }

                    this.NotificationMessage = notificationMessage;
                }
                else
                {
                    this.NotificationMessage = message.Exception.Message;
                }
            }
            else
            {
                this.NotificationMessage = message.Msg;
            }
        }

        public override void UpdateChanges(PresentationChangedMessage message)
        {
            base.UpdateChanges(message);
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

        #endregion
    }
}
