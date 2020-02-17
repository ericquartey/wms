using System.Threading.Tasks;

namespace Ferretto.VW.App.Modules.Operator.Services
{
    public interface IOperatorNavigationService
    {
        #region Methods

        void NavigateToDrawerView();

        void NavigateToOperatorMenu();

        #endregion
    }
}
