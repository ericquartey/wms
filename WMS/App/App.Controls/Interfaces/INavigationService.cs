using System;

namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface INavigationService
    {
        #region Properties

        bool IsUnitTest { get; set; }

        #endregion

        #region Methods

        void Appear<TViewModel>();

        INavigableView Appear(string moduleName, string viewModelName, object data = null);

        void Disappear(INavigableViewModel viewModel);

        void Disappear(INavigableView view);

        string GetNewViewModelName(string fullViewName);

        INavigableView GetRegisteredView(string instanceModuleViewName);

        INavigableViewModel GetRegisteredViewModel(string mapId, object data = null);

        INavigableView GetView(string moduleViewName, object data = null);

        INavigableView GetNewView(string moduleName, string viewModelName, object data);

        INavigableViewModel GetViewModelByName(string viewModelName);

        INavigableViewModel GetViewModelFromActiveWindow();

        void IsBusy(bool value);

        void LoadModule(string moduleName);

        void Register<TItemsView, TItemsViewModel>()
            where TItemsViewModel : INavigableViewModel
            where TItemsView : INavigableView;

        INavigableViewModel RegisterAndGetViewModel(string viewName, string token, object data = null);

        void StartPresentation(Action operationBefore, Action operationAfter);

        #endregion
    }
}
