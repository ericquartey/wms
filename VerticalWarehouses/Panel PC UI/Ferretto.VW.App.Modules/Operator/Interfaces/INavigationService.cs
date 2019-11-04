using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.Interfaces
{
    public interface INavigationService
    {
        #region Methods

        void NavigateFromView();

        void NavigateToView<T, TViewModel>()
          where T : BindableBase, TViewModel
          where TViewModel : IViewModel;

        void NavigateToView<T, TViewModel>(object parameterObject)
          where T : BindableBase, TViewModel
          where TViewModel : IViewModel;

        void NavigateToViewWithoutNavigationStack<T, TViewModel>()
            where T : BindableBase, TViewModel
            where TViewModel : IViewModel;

        #endregion
    }
}
