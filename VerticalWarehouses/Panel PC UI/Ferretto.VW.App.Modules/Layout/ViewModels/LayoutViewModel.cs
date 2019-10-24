using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    internal sealed class LayoutViewModel : ViewModelBase, IBusyViewModel
    {
        #region Fields

        private bool isBusy;

        #endregion

        #region Properties

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        #endregion
    }
}
