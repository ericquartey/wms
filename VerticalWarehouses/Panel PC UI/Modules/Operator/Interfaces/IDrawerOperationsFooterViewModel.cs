using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.Interfaces
{
    public interface IDrawerOperationsFooterViewModel : IViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion
    }
}
