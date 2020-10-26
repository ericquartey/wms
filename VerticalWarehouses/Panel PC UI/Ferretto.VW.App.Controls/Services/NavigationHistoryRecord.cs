namespace Ferretto.VW.App.Controls.Services
{
    internal class NavigationHistoryRecord
    {
        #region Constructors

        public NavigationHistoryRecord(string moduleName, string viewName, string viewModelName, int? id, NotificationMessage notificationMessage = null)
        {
            this.ModuleName = moduleName ?? throw new System.ArgumentException(Resources.General.ParameterCannotBeNullOrEmpty, nameof(moduleName));
            this.ViewName = viewName ?? throw new System.ArgumentException(Resources.General.ParameterCannotBeNullOrEmpty, nameof(viewName));
            this.ViewModelName = viewModelName ?? throw new System.ArgumentException(Resources.General.ParameterCannotBeNullOrEmpty, nameof(viewModelName));
            this.Id = id;
            this.NotificationMessage = notificationMessage;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Tag Id. It is used to contain the Id of loading unit, used by the loadingUnit view inside the navigation service.
        /// </summary>
        public int? Id { get; }

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
