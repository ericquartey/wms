namespace Ferretto.VW.App.Services
{
    public class NavigationCompletedEventArgs
    {
        #region Constructors

        public NavigationCompletedEventArgs(string moduleName, string viewModelName)
        {
            if (string.IsNullOrEmpty(viewModelName))
            {
                throw new System.ArgumentException("message", nameof(viewModelName));
            }

            if (string.IsNullOrEmpty(moduleName))
            {
                throw new System.ArgumentException("message", nameof(moduleName));
            }

            this.ViewModelName = viewModelName;
            this.ModuleName = moduleName;
        }

        #endregion

        #region Properties

        public string ModuleName { get; }

        public string ViewModelName { get; }

        #endregion
    }
}
