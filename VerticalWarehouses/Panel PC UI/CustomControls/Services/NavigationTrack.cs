namespace Ferretto.VW.App.Controls.Services
{
    public class NavigationTrack
    {
        #region Constructors

        public NavigationTrack(string moduleName, string viewName, string viewModelName, bool canBackTrack)
        {
            if (string.IsNullOrEmpty(moduleName))
            {
                throw new System.ArgumentException("message", nameof(moduleName));
            }

            if (string.IsNullOrEmpty(viewName))
            {
                throw new System.ArgumentException("message", nameof(viewName));
            }

            if (string.IsNullOrEmpty(viewModelName))
            {
                throw new System.ArgumentException("message", nameof(viewModelName));
            }

            this.ModuleName = moduleName;
            this.ViewName = viewName;
            this.ViewModelName = viewModelName;
            this.CanBackTrack = canBackTrack;
        }

        #endregion

        #region Properties

        public bool CanBackTrack { get; }

        public string ModuleName { get; }

        public string ViewModelName { get; }

        public string ViewName { get; }

        #endregion

        #region Methods

        public override string ToString()
        {
            return $"{this.ModuleName}.{this.ViewModelName} (track: {this.CanBackTrack})";
        }

        #endregion
    }
}
