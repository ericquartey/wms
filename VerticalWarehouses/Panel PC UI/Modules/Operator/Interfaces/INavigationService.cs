using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Modules.Operator.Interfaces
{
    public interface INavigationService
    {
        #region Methods

        void NavigateFromView();

        void NavigateToView<T, I>()
          where T : BindableBase, I
          where I : IViewModel;

        void NavigateToView<T, I>(object parameterObject)
          where T : BindableBase, I
          where I : IViewModel;

        void NavigateToViewWithoutNavigationStack<T, I>()
            where T : BindableBase, I
            where I : IViewModel;

        #endregion
    }
}
