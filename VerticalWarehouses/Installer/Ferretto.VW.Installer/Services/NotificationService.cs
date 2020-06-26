using Ferretto.VW.Installer.Core;

#nullable enable

namespace Ferretto.VW.Installer.Services
{
    public sealed class NotificationService : BindableBase, INotificationService
    {
        #region Fields

        private static readonly NotificationService instance = new NotificationService();

        private readonly NLog.ILogger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool isError;

        private string? message;

        #endregion

        #region Constructors

        private NotificationService()
        { }

        #endregion

        #region Properties

        public bool IsError
        {
            get => this.isError;
            private set => this.SetProperty(ref this.isError, value);
        }

        public string? Message
        {
            get => this.message;
            private set => this.SetProperty(ref this.message, value);
        }

        #endregion

        #region Methods

        public void ClearMessage()
        {
            this.Message = null;
            this.IsError = false;
        }

        public void SetErrorMessage(string message)
        {
            this.IsError = true;
            this.Message = message;

            this.logger.Error(message);
        }

        public void SetMessage(string message)
        {
            this.IsError = false;
            this.Message = message;

            this.logger.Info(message);
        }

        internal static INotificationService GetInstance()
        {
            return instance;
        }

        #endregion
    }
}
