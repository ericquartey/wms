using Ferretto.VW.App.Modules.Layout.Presentation;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    public class FooterViewModel : BasePresentationViewModel
    {
        #region Fields

        private string notificationMessage;

        #endregion

        #region Properties

        public string NotificationMessage
        {
            get => this.notificationMessage;
            set => this.SetProperty(ref this.notificationMessage, value);
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

            if (!string.IsNullOrEmpty(message.NotificationMessage))
            {
                this.NotificationMessage = message.NotificationMessage;
            }
            else if (message.Exception != null)
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

                case PresentationMode.Installator:
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
