namespace Ferretto.VW.App.Services
{
    public class NavigationCompletedPubSubEventArgs
    {
        #region Constructors

        public NavigationCompletedPubSubEventArgs(string moduleName, string viewModelName)
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
