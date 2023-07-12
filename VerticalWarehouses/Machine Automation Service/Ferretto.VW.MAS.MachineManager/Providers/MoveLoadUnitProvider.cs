using System;
using System.Linq;
using Ferretto.VW.CommonUtils;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.Providers
{
    internal sealed class MoveLoadUnitProvider : BaseProvider, IMoveLoadUnitProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly ICellsProvider cellsProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        #endregion

        #region Constructors

        public MoveLoadUnitProvider(
            ICellsProvider cellsProvider,
            IBaysDataProvider baysDataProvider,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IEventAggregator eventAggregator)
            : base(eventAggregator)
        {
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadUnitId: null,
                    insertLoadUnit: false,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadingUnitId,
                    insertLoadUnit: false,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadingUnitId,
                    insertLoadUnit: true,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
                    CommandAction.Activate),
                $"Bay {requestingBay} requested to activate move Loading unit {loadingUnitId} to Bay {requestingBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        // used by all guided movements
        public void EjectFromCell(MissionType missionType, LoadingUnitLocation destinationBay, int loadingUnitId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                 new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    destinationBay,
                    sourceCellId: null,
                    destinationCellId: null,
                    loadingUnitId,
                    loadUnitHeight: null,
                    netWeight: null,
                    insertLoadUnit: false),
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
                    sourceCellId: null,
                    destinationCellId,
                    loadingUnitId,
                    loadUnitHeight: null,
                    netWeight: null,
                    insertLoadUnit: true),
                $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to destination Cell {destinationCellId}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveFromBayToBay(MissionType missionType, LoadingUnitLocation sourceBay, LoadingUnitLocation destinationBay, BayNumber requestingBay, MessageActor sender)
        {
            var bay = this.baysDataProvider.GetByLoadingUnitLocation(sourceBay);
            var loadUnit = bay?.Positions.FirstOrDefault(x => x.Location == sourceBay)?.LoadingUnit;

            this.SendCommandToMachineManager(
            new MoveLoadingUnitMessageData(
                missionType,
                sourceBay,
                destinationBay,
                sourceCellId: null,
                destinationCellId: null,
                loadUnit?.Id,
                insertLoadUnit: false),
            $"Bay {requestingBay} requested to move Loading unit in Bay {sourceBay} to Bay {destinationBay}",
            sender,
            MessageType.MoveLoadingUnit,
            requestingBay);
        }

        public void MoveFromBayToCell(MissionType missionType, LoadingUnitLocation sourceBay, int? destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            var bay = this.baysDataProvider.GetByLoadingUnitLocation(sourceBay);
            var loadUnit = bay?.Positions.FirstOrDefault(x => x.Location == sourceBay)?.LoadingUnit;

            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    sourceBay,
                    LoadingUnitLocation.Cell,
                    sourceCellId: null,
                    destinationCellId,
                    loadUnit?.Id),
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

            this.loadingUnitsDataProvider.SetStartingCell(loadUnitId.Value, sourceCellId);

            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.Cell,
                    destinationBay,
                    sourceCellId,
                    destinationCellId: null,
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

        // destination can be elevator
        public void MoveLoadUnitToBay(MissionType missionType, int loadingUnitId, LoadingUnitLocation destination, BayNumber requestingBay, MessageActor sender)
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadingUnitId),
                $"Bay {requestingBay} requested to move Loading unit {loadingUnitId} to Bay {destination}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay);
        }

        public void MoveLoadUnitToCell(MissionType missionType, int loadingUnitId, int? destinationCellId, BayNumber requestingBay, MessageActor sender)
        {
            this.SendCommandToMachineManager(
                new MoveLoadingUnitMessageData(
                    missionType,
                    LoadingUnitLocation.LoadUnit,
                    LoadingUnitLocation.Cell,
                    sourceCellId: null,
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadUnitId: null,
                    insertLoadUnit: false,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadUnitId: null,
                    insertLoadUnit: false,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
                    CommandAction.Resume),
                $"Bay {requestingBay} requested to resume move Loading unit on Bay {targetBay}",
                sender,
                MessageType.MoveLoadingUnit,
                requestingBay,
                targetBay);
        }

        public void ResumeMoveLoadUnit(int? missionId, LoadingUnitLocation sourceBay, LoadingUnitLocation destination, BayNumber targetBay, int? wmsId, MissionType missionType, MessageActor sender)
        {
            var data = new MoveLoadingUnitMessageData(
                    missionType,
                    sourceBay,
                    destination,
                    sourceCellId: null,
                    destinationCellId: null,
                    loadUnitId: null,
                    insertLoadUnit: false,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
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
                    sourceCellId: null,
                    destinationCellId: null,
                    loadUnitId: null,
                    insertLoadUnit: false,
                    missionId,
                    loadUnitHeight: null,
                    netWeight: null,
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
