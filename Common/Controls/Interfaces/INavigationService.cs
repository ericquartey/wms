namespace Ferretto.Common.Controls.Interfaces
{
  public interface INavigationService
  {
    void Register<TItemsView, TItemsViewModel>() where TItemsViewModel : INavigableViewModel
                                                 where TItemsView : INavigableView;
    INavigableViewModel RegisterAndGetViewModel(string viewName, string token);
    INavigableViewModel GetViewModelByName(string viewModelname);
    INavigableViewModel GetViewModelByMapId(string viewModelname);
    void Appear<TViewModel>();
    void Appear(string module, string viewModel);
    void Disappear<TViewModel>();
    void Disappear(string module, string viewModel);
  }
}
