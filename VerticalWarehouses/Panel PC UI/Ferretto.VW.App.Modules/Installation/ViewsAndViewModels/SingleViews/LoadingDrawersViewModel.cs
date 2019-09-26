using Ferretto.VW.App.Controls;

namespace Ferretto.VW.App.Installation.ViewModels
{
    public class LoadingDrawersViewModel : BaseMainViewModel
    {
        #region Constructors

        public LoadingDrawersViewModel()
            : base(Services.PresentationMode.Installer)
        {
        }

        #endregion
    }
}
