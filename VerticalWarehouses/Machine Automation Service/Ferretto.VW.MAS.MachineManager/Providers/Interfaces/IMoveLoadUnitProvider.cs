using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;

namespace Ferretto.VW.MAS.MachineManager.Providers.Interfaces
{
    public interface IMoveLoadUnitProvider
    {
        #region Methods

        void AbortMove(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void ActivateMove(int? missionId, MissionType missionType, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void ActivateMoveToCell(int? missionId, MissionType missionType, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void EjectFromCell(MissionType missionType, LoadingUnitLocation destinationBay, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void InsertToCell(MissionType missionType, LoadingUnitLocation sourceBay, int? destinationCellId, int loadingUnitId, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToBay(MissionType missionType, LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromBayToCell(MissionType missionType, LoadingUnitLocation sourceBay, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToBay(MissionType missionType, int? sourceCellId, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender);

        void MoveFromCellToCell(MissionType missionType, int? sourceCellId, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void MoveLoadUnitToBay(MissionType missionType, int loadingUnitId, LoadingUnitLocation destination, BayNumber requestingBay, MessageActor sender);

        void MoveLoadUnitToCell(MissionType missionType, int loadingUnitId, int? destinationCellId, BayNumber requestingBay, MessageActor sender);

        void PauseMove(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void RemoveLoadUnit(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        void ResumeMoveLoadUnit(int? missionId, LoadingUnitLocation sourceBay, LoadingUnitLocation destination, BayNumber targetBay, int? wmsId, MissionType missionType, MessageActor sender);

        void StopMove(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender);

        #endregion
    }
}
