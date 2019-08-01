using Ferretto.VW.Utils.Interfaces;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface ISSMainViewModel : IViewModel
    {
        #region Properties

        IViewModel SSNavigationRegionCurrentViewModel { get; set; }

        #endregion
    }
}
