using Prism.Commands;

namespace Ferretto.VW.InstallationApp
{
    public interface IMainWindowBackToIAPPButtonViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion
    }
}
