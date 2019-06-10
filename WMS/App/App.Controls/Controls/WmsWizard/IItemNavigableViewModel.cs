using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls
{
    public interface IItemNavigableViewModel : INavigableViewModel
    {
        #region Methods

        bool CanGoToNextView();

        (string moduleName, string viewName, object data) GetNextView();

        bool Save();

        #endregion
    }
}
