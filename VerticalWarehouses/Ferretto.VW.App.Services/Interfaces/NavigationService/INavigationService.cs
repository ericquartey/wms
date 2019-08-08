namespace Ferretto.VW.App.Services.Interfaces
{
    public interface INavigationService
    {
        #region Methods

        void Appear(string moduleName, string viewModelName, object data = null);

        void Disappear(INavigableViewModel viewModel);

        void GoBack();

        void LoadModule(string moduleName);

        void SetBusy(bool isBusy);

        #endregion
    }
}
