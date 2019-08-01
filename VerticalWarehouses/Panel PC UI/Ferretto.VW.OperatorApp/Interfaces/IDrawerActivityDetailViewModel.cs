using Ferretto.VW.Utils.Interfaces;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Operator.Interfaces
{
    public interface IDrawerActivityDetailViewModel : IViewModel
    {
        #region Properties

        DrawerActivityItemDetail ItemDetail { get; set; }

        #endregion
    }
}
