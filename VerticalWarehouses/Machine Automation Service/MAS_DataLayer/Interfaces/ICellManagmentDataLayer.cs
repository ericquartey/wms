using Ferretto.VW.MAS.DataModels.Cells;
using Ferretto.VW.MAS.DataModels.LoadingUnits;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ICellManagmentDataLayer
    {
        #region Methods

        CellStatisticsSummary GetCellStatistics();

        ///// <summary>
        /////     Get an object with the vertical position and side to place a drawer
        ///// </summary>
        ///// <param name="loadingUnitHeight"></param>
        ///// <param name="loadingUnitId">Id of the Drawer we take into account</param>
        ///// <returns>An object with position and side for a return mission</returns>
        //LoadingUnitPosition GetFreeBlockPosition(decimal loadingUnitHeight, int loadingUnitId);

        /// <summary>
        /// Get to the mission the lowest cell position for a drawer
        /// </summary>
        /// <param name="cellId">Id of the lowest drawer cell</param>
        /// <returns>The drawer side and height</returns>
        LoadingUnitPosition GetLoadingUnitPosition(int cellId);

        #endregion

        ///// <summary>
        ///// This methods is been invoked when a drawer backs from the bay to cells
        ///// </summary>
        ///// <param name="loadingUnitId">Id of the Drawer we take into account</param>
        //void SetReturnLoadingUnitInLocation(int loadingUnitId);

        ///// <summary>
        ///// Set the status of a cell, when a drawer free some cells
        ///// </summary>
        ///// <param name="loadingUnitId">Id of the Drawer we take into account</param>
        //void SetWithdrawalLoadingUnitFromLocation(int loadingUnitId);
    }
}
