using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IMoveLoadingUnitProvider
    {
        #region Methods

        void AbortMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void EjectFromCell(LoadingUnitLocation destinationBay, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void InsertToCell(LoadingUnitLocation sourceBay, int destinationCellId, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToBay(LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToCell(LoadingUnitLocation sourceBay, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToBay(int? sourceCellId, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToCell(int? sourceCellId, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveLoadingUnitToBay(int loadingUnitId, LoadingUnitLocation destination, BayNumber requestingBay, MessageActor sender);

        void MoveLoadingUnitToCell(int loadingUnitId, int destinationCellId, BayNumber requestingBay, MessageActor sender);

        void PauseMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void ResumeMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void StopMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        #endregion
    }
}
