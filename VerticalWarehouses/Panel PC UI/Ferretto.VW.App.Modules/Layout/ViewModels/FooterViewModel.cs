using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Models;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    public class FooterViewModel : BasePresentationViewModel
    {
        #region Fields

        private string notificationMessage;

        private NotificationSeverity notificationSeverity;

        #endregion

        #region Properties

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
                           message.Exception.Message;
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

        #endregion
    }
}
