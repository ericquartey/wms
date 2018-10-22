using System.Windows.Input;

namespace Ferretto.Common.Controls.Interfaces
{
    public interface IWmsGridViewModel
    {
        #region Properties

        ICommand CmdRefresh { get; set; }

        #endregion Properties
    }
}
