namespace Ferretto.VW.App.Controls.Services
{
    public class NavigationTrack
    {
        #region Fields

        // TODO remove this constant
        private const string ErrorMessage = "Parameter cannot be null or empty";

        #endregion

        #region Constructors

        public NavigationTrack(string moduleName, string viewName, string viewModelName)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new System.ArgumentException(ErrorMessage, nameof(moduleName));
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new System.ArgumentException(ErrorMessage, nameof(viewName));
            }

            if (string.IsNullOrEmpty(viewModelName))
            {
                throw new System.ArgumentException(ErrorMessage, nameof(viewModelName));
            }

            this.ModuleName = moduleName;
            this.ViewName = viewName;
            this.ViewModelName = viewModelName;
        }

        #endregion

        #region Properties

        public bool IsTrackable { get; set; }

        public string ModuleName { get; }

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
