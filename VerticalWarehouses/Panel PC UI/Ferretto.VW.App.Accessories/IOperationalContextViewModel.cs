using System.Threading.Tasks;

namespace Ferretto.VW.App.Accessories
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
