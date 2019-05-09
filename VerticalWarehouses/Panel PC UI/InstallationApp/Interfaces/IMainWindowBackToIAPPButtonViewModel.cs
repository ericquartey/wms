using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.InstallationApp
{
    public interface IMainWindowBackToIAPPButtonViewModel : IViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion
    }
}
