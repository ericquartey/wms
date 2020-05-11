namespace Ferretto.VW.App.Controls.Services
{
    internal class NavigationHistoryRecord
    {
        #region Constructors

        public NavigationHistoryRecord(string moduleName, string viewName, string viewModelName, NotificationMessage notificationMessage = null)
        {
            this.ModuleName = moduleName ?? throw new System.ArgumentException(Resources.General.ParameterCannotBeNullOrEmpty, nameof(moduleName));
            this.ViewName = viewName ?? throw new System.ArgumentException(Resources.General.ParameterCannotBeNullOrEmpty, nameof(viewName));
            this.ViewModelName = viewModelName ?? throw new System.ArgumentException(Resources.General.ParameterCannotBeNullOrEmpty, nameof(viewModelName));
            this.NotificationMessage = notificationMessage;
        }

        #endregion

        #region Properties

        public bool IsTrackable { get; set; }

        public string ModuleName { get; }

        public NotificationMessage NotificationMessage { get; }

        public string ViewModelName { get; }

        public string ViewName { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.ModuleName}.{this.ViewName}";
        }

        #endregion
    }
}
