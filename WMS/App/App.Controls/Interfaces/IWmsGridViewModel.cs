using System.Windows.Input;

namespace Ferretto.WMS.App.Controls.Interfaces
{
    public interface IWmsGridViewModel
    {
        #region Properties

        ICommand CmdRefresh { get; set; }

        #endregion
    }
}
