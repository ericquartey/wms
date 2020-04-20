using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class AlphanumericBarSettingsViewModel : BaseMainViewModel
    {
        #region Constructors

        public AlphanumericBarSettingsViewModel()
            : base(PresentationMode.Installer)
        {
        }

        #endregion
    }
}
