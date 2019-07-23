using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Interfaces;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IDrawerActivityDetailViewModel : IViewModel
    {
        #region Properties

        DrawerActivityItemDetail ItemDetail { get; set; }

        #endregion
    }
}
