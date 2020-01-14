using System;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal sealed class MoveLoadingUnitProvider : BaseProvider, IMoveLoadingUnitProvider
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        #endregion

        #region Constructors

        public MoveLoadingUnitProvider(
            ICellsProvider cellsProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
        }

        #endregion

        #region Methods

        public void AbortMove(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    MissionType.NoType,
                    LoadingUnitLocation.NoLocation,
                    LoadingUnitLocation.NoLocation,
                    null,
                    null,
                    null,
                    false,
                    false,
                    missionId,
                    CommandAction.Abort),
                $"Bay {requestingBay} requested to abort move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        public void ActivateMove(int? missionId, MissionType missionType, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    LoadingUnitLocation.NoLocation,
                    null,
                    null,
                    loadingUnitId,
                    false,
                    false,
                    missionId,
                    CommandAction.Activate),
                $"Bay {requestingBay} requested to activate move Loading unit {loadingUnitId} to Bay {requestingBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void ActivateMoveToCell(int? missionId, MissionType missionType, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    LoadingUnitLocation.Cell,
                    null,
                    null,
                    loadingUnitId,
                    false,
                    false,
                    missionId,
                    CommandAction.Activate),
                $"Bay {requestingBay} requested to activate move Loading unit {loadingUnitId} to Bay {requestingBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void EjectFromCell(MissionType missionType, LoadingUnitLocation destinationBay, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                 new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    destinationBay,
                    null,
                    null,
                    loadingUnitId,
                    false,
                    true),
                 $"Bay {requestingBay} requested to eject Loading unit {loadingUnitId} to Bay {destinationBay}",
                 sender,
                 MessageType.MoveLoadingUnit,
                 requestingBay);
        }

        // sourceBay can be also Elevator
        public void InsertToCell(MissionType missionType, LoadingUnitLocation sourceBay, int? destinationCellId, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            if (sourceBay is LoadingUnitLocation.Cell)
            {
                throw new ArgumentException(sourceBay.ToString());
            }

            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
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

        public void MoveFromBayToBay(MissionType missionType, LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
            new MoveLoadingUnitMessageData(
                missionType,
                sourceBay,
                destinationBay,
                null,
                null,
                null,
                false,
                true),
            $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to Bay {destinationBay}",
            sender,
            MessageType.MoveLoadingUnit,
            requestingBay);
        }

        public void MoveFromBayToCell(MissionType missionType, LoadingUnitLocation sourceBay, int? destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
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

        public void MoveFromCellToBay(MissionType missionType, int? sourceCellId, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            int? loadUnitId = null;
            if (sourceCellId.HasValue)
            {
                loadUnitId = this.cellsProvider.GetById(sourceCellId.Value)?.LoadingUnit?.Id;
            }
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.Cell,
                    destinationBay,
                    sourceCellId,
                    null,
                    loadUnitId),
                $"Bay {requestingBay} requested to move Loading unit in Cell {sourceCellId} to destination {destinationBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromCellToCell(MissionType missionType, int? sourceCellId, int? destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            int? loadUnitId = null;
            if (sourceCellId.HasValue)
            {
                loadUnitId = this.cellsProvider.GetById(sourceCellId.Value)?.LoadingUnit?.Id;
            }
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.Cell,
                    LoadingUnitLocation.Cell,
                    sourceCellId,
                    destinationCellId,
                    loadUnitId),
                $"Bay {requestingBay} requested to move Loading unit in Cell {sourceCellId} to Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveLoadingUnitToBay(MissionType missionType, int loadingUnitId, LoadingUnitLocation destination, BayNumber requestingBay, MessageActor sender)
        {
            if (destination is LoadingUnitLocation.Cell)
            {
                throw new ArgumentException(destination.ToString());
            }

            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    destination,
                    null,
                    null,
                    loadingUnitId),
                $"Bay {requestingBay} requested to move Loading unit {loadingUnitId} to Bay {destination}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveLoadingUnitToCell(MissionType missionType, int loadingUnitId, int destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    LoadingUnitLocation.Cell,
                    null,
                    destinationCellId,
                    loadingUnitId),
                $"Bay {requestingBay} requested to move Loading unit {loadingUnitId} to Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void PauseMove(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    MissionType.NoType,
                    LoadingUnitLocation.NoLocation,
                    LoadingUnitLocation.NoLocation,
                    null,
                    null,
                    null,
                    false,
                    false,
                    missionId,
                    CommandAction.Pause),
                $"Bay {requestingBay} requested to pause move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        public void RemoveLoadUnit(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    MissionType.NoType,
                    LoadingUnitLocation.NoLocation,
                    LoadingUnitLocation.NoLocation,
                    null,
                    null,
                    null,
                    false,
                    false,
                    missionId,
                    CommandAction.Resume),
                $"Bay {requestingBay} requested to resume move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        public void ResumeMoveLoadUnit(int? missionId, LoadingUnitLocation sourceBay, LoadingUnitLocation destination, BayNumber targetBay, int? wmsId, MessageActor sender)
        {
            var data = new MoveLoadingUnitMessageData(
                    (wmsId.HasValue ? MissionType.WMS : MissionType.Manual),
                    sourceBay,
                    destination,
                    null,
                    null,
                    null,
                    false,
                    false,
                    missionId,
                    CommandAction.Resume);
            data.WmsId = wmsId;
            this.SendCommandToMachineManager(
                data,
                $"Bay {sourceBay} requested to resume move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                targetBay);
        }

        public void StopMove(int? missionId, BayNumber requestingBay, BayNumber targetBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    MissionType.NoType,
                    LoadingUnitLocation.NoLocation,
                    LoadingUnitLocation.NoLocation,
                    null,
                    null,
                    null,
                    false,
                    false,
                    missionId,
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
