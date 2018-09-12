namespace Ferretto.Common.Controls.Interfaces
{
    public interface INavigationService
    {
        void Register<TItemsView, TItemsViewModel>() where TItemsViewModel : INavigableViewModel
            where TItemsView : INavigableView;

        INavigableViewModel RegisterAndGetViewModel(string viewName, string token);
        INavigableViewModel GetViewModelByName(string viewModelName);
        INavigableViewModel GetViewModelByMapId(string mapId);
        void Appear<TViewModel>();
        void Appear(string moduleName, string viewModelName);
        void Disappear<TViewModel>();
        void Disappear(string moduleName, string viewModelName);
    }
}
