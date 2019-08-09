using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services.Interfaces;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    public class LayoutViewModel : ViewModelBase, IBusyViewModel
    {
        #region Constructors

        public LayoutViewModel()
        {
        }

        #endregion

        #region Properties

        public bool IsBusy { get; set; }

        #endregion
    }
}
