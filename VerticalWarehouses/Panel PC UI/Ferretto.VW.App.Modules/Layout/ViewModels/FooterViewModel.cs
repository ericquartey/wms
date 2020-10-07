using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;
using NLog;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    internal sealed class FooterViewModel : BasePresentationViewModel
    {
        #region Fields

        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

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

            var prevStep = this.GetInstance<PresentationSinglePageStep>();
            prevStep.Type = PresentationTypes.PrevStep;
            this.States.Add(prevStep);

            var nextStep = this.GetInstance<PresentationSinglePageStep>();
            nextStep.Type = PresentationTypes.NextStep;
            this.States.Add(nextStep);

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
                if (this.NotificationMessage != null)
                {
                    if (this.NotificationSeverity != NotificationSeverity.Success)
                    {
                        this.NotificationMessage = null;
                    }
                    else
                    {
                        _ = Task.Delay(1500).ContinueWith(t => this.NotificationMessage = null, TaskScheduler.Current);
                    }
                }

                return;
            }

            if (message.NotificationSeverity == NotificationSeverity.NotSpecified)
            {
                return;
            }

            this.NotificationSeverity = message.NotificationSeverity;

            if (message.Exception != null)
            {
                switch (message.Exception)
                {
                    case MasWebApiException<ProblemDetails> webApiException:
                        if (webApiException.Result is null)
                        {
                            this.NotificationMessage = webApiException.Message;
                        }
                        else
                        {
                            if (webApiException.Result.Detail is null)
                            {
                                this.NotificationMessage =
                                webApiException.Result.Title +
                                System.Environment.NewLine +
                                ((!(webApiException.Result is null)
                                 &&
                                 webApiException.Result.AdditionalProperties.Any()) ? webApiException.Result.AdditionalProperties.First().Value : string.Empty);
                            }
                            else
                            {
                                this.NotificationMessage =
                                webApiException.Result.Title +
                                System.Environment.NewLine +
                                webApiException.Result.Detail?.Split('\n', '\r').FirstOrDefault();
                            }
                        }

                        break;

                    case MasWebApiException webApiException:
                        {
                            var notificationMessage = Resources.Localized.Get("General.ErrorCommunicatingWithServices");

                            if (webApiException.InnerException != null)
                            {
                                notificationMessage +=
                                    System.Environment.NewLine +
                                    webApiException.InnerException.Message;
                            }
                            else
                            {
                                try
                                {
                                    var problemDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<ProblemDetails>(webApiException.Response);

                                    notificationMessage +=
                                       System.Environment.NewLine +
                                       problemDetails.Detail ?? problemDetails.Title;
                                }
                                catch
                                {
                                    // do nothing
                                }
                            }

                            this.NotificationMessage = notificationMessage;
                            break;
                        }

                    default:
                        this.NotificationMessage = message.Exception.Message;
                        break;
                }

                this.logger.Error(message.Exception);
            }
            else
            {
                this.NotificationMessage = message.Msg;

                switch (message.NotificationSeverity)
                {
                    case NotificationSeverity.Error:
                        this.logger.Error(message.Msg);
                        break;

                    case NotificationSeverity.Warning:
                        this.logger.Warn(message.Msg);
                        break;

                    case NotificationSeverity.Info:
                    case NotificationSeverity.Success:
                        this.logger.Info(message.Msg);
                        break;
                }
            }
        }

        public override void UpdateChanges(PresentationChangedMessage message)
        {
            message.States?.ForEach((s) =>
            {
                if (s != null)
                {
                    this.Show(s.Type, s.IsVisible ?? false);
                }
            });
        }

        public override void UpdatePresentation(PresentationMode mode)
        {
            if (mode == PresentationMode.None ||
                this.CurrentPresentation == mode)
            {
                return;
            }

            this.CurrentPresentation = mode;

            this.Show(PresentationTypes.None, false);
            this.Show(PresentationTypes.Back, true);

            switch (mode)
            {
                case PresentationMode.Login:
                case PresentationMode.Menu:
                    this.Show(PresentationTypes.Back, false);
                    break;

                case PresentationMode.Installer:
                case PresentationMode.Operator:
                case PresentationMode.Help:
                    break;
            }
        }

        #endregion
    }
}
