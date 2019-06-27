using System.Threading.Tasks;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls
{
    public interface IStepNavigableViewModel : INavigableViewModel
    {
        #region Properties

        string Title { get; set; }

        #endregion

        #region Methods

        bool CanGoToNextView();

        bool CanSave();

        string GetError();

        (string moduleName, string viewName, object data) GetNextView();

        Task<bool> SaveAsync();

        #endregion
    }
}
