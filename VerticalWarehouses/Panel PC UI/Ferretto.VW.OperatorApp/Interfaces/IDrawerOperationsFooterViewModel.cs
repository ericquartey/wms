using System.Windows.Input;
using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IDrawerOperationsFooterViewModel : IViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion
    }
}
