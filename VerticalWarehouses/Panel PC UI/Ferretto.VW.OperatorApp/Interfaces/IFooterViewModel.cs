using System.Windows.Input;
using Ferretto.VW.Utils.Interfaces;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IFooterViewModel : IViewModel
    {
        #region Properties

        ICommand NavigateBackCommand { get; }

        string Note { get; set; }

        #endregion

        #region Methods

        void FinalizeBottomButtons();

        #endregion
    }
}
