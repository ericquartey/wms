using Ferretto.VW.Utils.Interfaces;

namespace Ferretto.VW.InstallationApp
{
    public interface IFooterViewModel : IViewModel
    {
        #region Properties

        string Note { get; set; }

        #endregion
    }
}
