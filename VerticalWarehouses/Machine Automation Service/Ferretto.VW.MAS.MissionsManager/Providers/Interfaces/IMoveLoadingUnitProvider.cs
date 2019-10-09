using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MissionsManager.Providers.Interfaces
{
    public interface IMoveLoadingUnitProvider
    {
        #region Methods

        void MoveFromBayToBay(LoadingUnitDestination sourceBay, LoadingUnitDestination destinationBay, BayNumber requestingBay);

        void MoveFromBayToCell(LoadingUnitDestination sourceBay, int destinationCellId, BayNumber requestingBay);

        void MoveFromCellToBay(int sourceCellId, LoadingUnitDestination destinationBay, BayNumber requestingBay);

        void MoveFromCellToCell(int sourceCellId, int destinationCellId, BayNumber requestingBay);

        void MoveLoadingUnitToBay(int loadingUnitId, LoadingUnitDestination destination, BayNumber requestingBay);

        void MoveLoadingUnitToCell(int loadingUnitId, int destinationCellId, BayNumber requestingBay);

        void StopMoving();

        #endregion
    }
}
