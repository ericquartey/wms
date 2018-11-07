using System.Windows.Input;

namespace Ferretto.Common.Controls
{
    public interface IEdit
    {
        #region Properties

        ICommand RevertCommand { get; }
        ICommand SaveCommand { get; }

        #endregion Properties
    }
}
