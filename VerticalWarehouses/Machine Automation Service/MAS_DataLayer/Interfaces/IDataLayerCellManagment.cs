namespace Ferretto.VW.MAS_DataLayer
{
    public interface IDataLayerCellManagment
    {
        #region Methods

        /// <summary>
        ///     Get an object with the vertical position and side to place a drawer
        /// </summary>
        /// <param name="drawerHeight">Drawer height to insert in the magazine</param>
        /// <param name="loadingUnitId">Id of the Drawer we take into account</param>
        /// <returns>An object with position and side for a return mission</returns>
        LoadingUnitPosition GetFreeBlockPosition(decimal loadingUnitHeight, int loadingUnitId);

        /// <summary>
        /// Get to the mission the lowest cell position for a drawer
        /// </summary>
        /// <param name="cellId">Id of the lowest drawer cell</param>
        /// <returns>The dawer side and height</returns>
        LoadingUnitPosition GetLoadingUnitPosition(int cellId);

        /// <summary>
        /// This methods is been invoked when a drawer backs from the bay to cells
        /// </summary>
        /// <param name="loadingUnitId">Id of the Drawer we take into account</param>
        /// <exception cref="DataLayerExceptionEnum.NO_FREE_BLOCK_BOOKED_EXCEPTION">Thrown when a drawer backs from bay, but we don't find booked cells in a Free Blocks table</exception>
        /// <exception cref="DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION">Thrown when we have booked cells in the Free Blocks table, but we don't find one of them in the cells table</exception>
        void SetReturnLoadingUnitInLocation(int loadingUnitId);

        /// <summary>
        /// Set the status of a cell, when a drawer free some cells
        /// </summary>
        /// <param name="loadingUnitId">Id of the Drawer we take into account</param>
        /// <exception cref="DataLayerExceptionEnum.CELL_NOT_FOUND_EXCEPTION">Thrown when we don't find cells in the Free Blocks table</exception>
        void SetWithdrawalLoadingUnitFromLocation(int loadingUnitId);

        #endregion
    }
}
