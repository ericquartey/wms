using Ferretto.VW.Utils.Interfaces;

namespace Ferretto.VW.App.Installation.Interfaces
{
    public interface IFooterViewModel : IViewModel
    {
        #region Properties

        string Note { get; set; }

        #endregion
    }
}
