using Ferretto.VW.Utils.Interfaces;
using Ferretto.VW.WmsCommunication.Source;

namespace Ferretto.VW.OperatorApp.Interfaces
{
    public interface IDrawerActivityDetailViewModel : IViewModel
    {
        #region Properties

        DrawerActivityItemDetail ItemDetail { get; set; }

        #endregion
    }
}
