using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
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
                    null,
                    null,
                    null,
                    false,
                    false,
                    CommandAction.Abort),
                $"Bay {requestingBay} requested to abort move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        public void EjectFromCell(int sourceCellId, LoadingUnitLocation destinationBay, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                 new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.Cell,
                    destinationBay,
                    sourceCellId,
                    null,
                    loadingUnitId,
                    false,
                    true),
                 $"Bay {requestingBay} requested to move Loading unit to Bay {destinationBay} from Cell {sourceCellId}",
                 sender,
                 MessageType.MoveLoadingUnit,
                 requestingBay);
        }

        public void InsertToCell(LoadingUnitLocation sourceBay, int destinationCellId, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    sourceBay,
                    LoadingUnitLocation.Cell,
                    null,
                    destinationCellId,
                    loadingUnitId,
                    true),
                $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to destination Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromBayToBay(LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
            new MoveLoadingUnitMessageData(
                sourceBay,
                destinationBay,
                null,
                null,
                null),
            $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to Bay {destinationBay}",
            sender,
            MessageType.MoveLoadingUnit,
            requestingBay);
        }

        public void MoveFromBayToCell(LoadingUnitLocation sourceBay, int? destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    sourceBay,
                    LoadingUnitLocation.Cell,
                    null,
                    destinationCellId,
                    null),
                $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to destination Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromCellToBay(int? sourceCellId, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.Cell,
                    destinationBay,
                    sourceCellId,
                    null,
                    null),
                $"Bay {requestingBay} requested to move Loading unit in Cell {sourceCellId} to destination {destinationBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromCellToCell(int? sourceCellId, int? destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMissionManager(
                new MoveLoadingUnitMessageData(
                    LoadingUnitLocation.Cell,
                    LoadingUnitLocation.Cell,
                    sourceCellId,
                    destinationCellId,
                    null),
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
                    null,
                    null,
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
                    null,
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
                    null,
                    null,
                    null,
                    false,
                    false,
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
