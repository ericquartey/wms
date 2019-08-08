using Ferretto.VW.Utils.Interfaces;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.Interfaces
{
    public interface IStatisticsFooterViewModel : IViewModel
    {
        #region Properties

        CompositeCommand BackButtonCommand { get; set; }

        #endregion
    }
}
