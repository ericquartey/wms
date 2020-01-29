using System.Threading.Tasks;

namespace Ferretto.VW.App.Modules.Operator.Services
{
    public interface IOperatorNavigationService
    {
        #region Methods

        Task NavigateToDrawerViewAsync();

        Task NavigateToOperatorMenuAsync();

        #endregion
    }
}
