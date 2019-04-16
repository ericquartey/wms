using System.Windows.Input;

namespace Ferretto.WMS.App.Controls
{
    public interface IEdit
    {
        #region Properties

        ICommand RevertCommand { get; }

        ICommand SaveCommand { get; }

        #endregion
    }
}
