using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MissionsManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionsManager.Providers
{
    internal class MoveLoadingUnitProvider : BaseProvider, IMoveLoadingUnitProvider
    {
        #region Constructors

        public MoveLoadingUnitProvider(IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
        }

        #endregion

        #region Methods

        public void AbortMove(BayNumber requestingBay, BayNumber targetBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.NoLocation,
                    LoadingUnitLocation.NoLocation,
                    -1,
                    -1,
                    -1,
                    CommandAction.Abort),
                $"Bay {requestingBay} requested to abort move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        public void MoveFromBayToBay(LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
            new MoveLoadingUnitMessageData(
                sourceBay,
                destinationBay,
                -1,
                -1,
                -1),
            $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to Bay {destinationBay}",
            sender,
            MessageType.MoveLoadingUnit,
            requestingBay);
        }

        public void MoveFromBayToCell(LoadingUnitLocation sourceBay, int destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    sourceBay,
                    LoadingUnitLocation.Cell,
                    -1,
                    destinationCellId,
                    -1),
                $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to destination Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromCellToBay(int sourceCellId, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.Cell,
                    destinationBay,
                    sourceCellId,
                    -1,
                    -1),
                $"Bay {requestingBay} requested to move Loading unit in Cell {sourceCellId} to destination {destinationBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromCellToCell(int sourceCellId, int destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.Cell,
                    LoadingUnitLocation.Cell,
                    sourceCellId,
                    destinationCellId,
                    -1),
                $"Bay {requestingBay} requested to move Loading unit in Cell {sourceCellId} to Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveLoadingUnitToBay(int loadingUnitId, LoadingUnitLocation destination, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.LoadingUnit,
                    destination,
                    -1,
                    -1,
                    loadingUnitId),
                $"Bay {requestingBay} requested to move Loading unit {loadingUnitId} to Bay {destination}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveLoadingUnitToCell(int loadingUnitId, int destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.LoadingUnit,
                    LoadingUnitLocation.Cell,
                    -1,
                    destinationCellId,
                    loadingUnitId),
                $"Bay {requestingBay} requested to move Loading unit {loadingUnitId} to Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void StopMove(BayNumber requestingBay, BayNumber targetBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.NoLocation,
                    LoadingUnitLocation.NoLocation,
                    -1,
                    -1,
                    -1,
                    CommandAction.Stop),
                $"Bay {requestingBay} requested to stop move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        #endregion
    }
}
