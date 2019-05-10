using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Ferretto.WMS.App.Controls
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1030:Use events where appropriate",
        Justification = "Ok")]
    public interface IWmsCommand : ICommand
    {
        #region Methods

        Task<bool> ExecuteAsync(object parameter = null);

        void RaiseCanExecuteChanged();

        #endregion
    }
}
