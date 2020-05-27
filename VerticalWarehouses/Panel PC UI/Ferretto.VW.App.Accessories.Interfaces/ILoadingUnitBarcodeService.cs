using System.Threading.Tasks;

namespace Ferretto.VW.App.Accessories.Interfaces
{
    public interface ILoadingUnitBarcodeService
    {
        #region Methods

        /// <summary>
        /// Processes a user action.
        /// </summary>
        /// <param name="e">The action arguments.</param>
        /// <returns>Returns True, if the action was processed, False otherwise.</returns>
        Task<bool> ProcessUserActionAsync(UserActionEventArgs e);

        #endregion
    }
}
