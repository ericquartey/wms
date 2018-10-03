namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigationService
    {
        #region Methods

        void Appear<TViewModel>();

        void Appear(string moduleName, string viewModelName);

        void Disappear(INavigableViewModel viewModel);

        INavigableViewModel GetRegisteredViewModel(string mapId);

        string GetViewModelBindFirstId(string fullViewName);

        INavigableViewModel GetViewModelByName(string viewModelName);

        void LoadModule(string moduleName);

        void Register<TItemsView, TItemsViewModel>() where TItemsViewModel : INavigableViewModel
                                                            where TItemsView : INavigableView;

        INavigableViewModel RegisterAndGetViewModel(string viewName, string token);

        #endregion Methods
    }
}
