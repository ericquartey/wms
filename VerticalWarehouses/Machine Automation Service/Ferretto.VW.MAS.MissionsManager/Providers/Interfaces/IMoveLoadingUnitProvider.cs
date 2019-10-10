using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MissionsManager.Providers.Interfaces
{
    public interface IMoveLoadingUnitProvider
    {
        #region Methods

        void AbortMove(BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void MoveFromBayToBay(LoadingUnitDestination sourceBay, LoadingUnitDestination destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToCell(LoadingUnitDestination sourceBay, int destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToBay(int sourceCellId, LoadingUnitDestination destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToCell(int sourceCellId, int destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveLoadingUnitToBay(int loadingUnitId, LoadingUnitDestination destination, BayNumber requestingBay, MessageActor sender);

        void MoveLoadingUnitToCell(int loadingUnitId, int destinationCellId, BayNumber requestingBay, MessageActor sender);

        void StopMove(BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        #endregion
    }
}
