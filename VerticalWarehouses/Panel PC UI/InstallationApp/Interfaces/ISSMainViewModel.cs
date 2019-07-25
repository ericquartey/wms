using Ferretto.VW.Utils.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface ISSMainViewModel : IViewModel
    {
        #region Properties

        BindableBase SSNavigationRegionCurrentViewModel { get; set; }

        #endregion
    }
}
