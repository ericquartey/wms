using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.Interfaces
{
    public interface IMainWindowViewModel
    {
        #region Properties

        BindableBase ContentRegionCurrentViewModel { get; set; }

        BindableBase ExitViewButtonRegionCurrentViewModel { get; set; }

        BindableBase NavigationRegionCurrentViewModel { get; set; }

        #endregion
    }
}
