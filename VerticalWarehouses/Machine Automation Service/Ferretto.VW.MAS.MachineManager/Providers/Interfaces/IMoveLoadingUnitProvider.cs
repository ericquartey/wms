using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IMoveLoadingUnitProvider
    {
        #region Methods

        void AbortMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void ActivateMove(Guid? missionId, LoadingUnitLocation sourceBay, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void EjectFromCell(MissionType missionType, LoadingUnitLocation destinationBay, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void InsertToCell(MissionType missionType, LoadingUnitLocation sourceBay, int destinationCellId, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToBay(MissionType missionType, LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToCell(MissionType missionType, LoadingUnitLocation sourceBay, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToBay(MissionType missionType, int? sourceCellId, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToCell(MissionType missionType, int? sourceCellId, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveLoadingUnitToBay(MissionType missionType, int loadingUnitId, LoadingUnitLocation destination, BayNumber requestingBay, MessageActor sender);

        void MoveLoadingUnitToCell(MissionType missionType, int loadingUnitId, int destinationCellId, BayNumber requestingBay, MessageActor sender);

        void PauseMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void RemoveLoadUnit(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void ResumeMoveLoadUnit(Guid? missionId, LoadingUnitLocation sourceBay, LoadingUnitLocation destination, BayNumber targetBay, int? wmsId, MessageActor sender);

        void StopMove(Guid? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        #endregion
    }
}
