using System.Threading.Tasks;
using Ferretto.VW.App.Accessories;

namespace Ferretto.VW.App.Services
{
    public interface IOperationalContextViewModel
    {
        #region Properties

        string ActiveContextName { get; }

        #endregion

        #region Methods

        Task CommandUserActionAsync(UserActionEventArgs userAction);

        #endregion
    }
}
