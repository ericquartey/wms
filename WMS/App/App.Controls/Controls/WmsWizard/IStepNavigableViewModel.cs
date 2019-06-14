using System.Threading.Tasks;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls
{
    public interface IStepNavigableViewModel : INavigableViewModel
    {
        #region Methods

        bool CanGoToNextView();

        bool CanSave();

        (string moduleName, string viewName, object data) GetNextView();

        string GetError();

        Task<bool> SaveAsync();

        #endregion
    }
}
