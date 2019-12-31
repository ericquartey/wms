using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Prism.Events;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly ICellsProvider cellsProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitsDataProvider;

        private readonly ILogger<MissionSchedulingService> logger;

        private readonly IMachineMissionsProvider machineMissionsProvider;

        private readonly IMissionsDataProvider missionsDataProvider;

        private readonly NotificationEvent notificationEvent;

        #endregion

        #region Constructors

        public MissionSchedulingProvider(
            ICellsProvider cellsProvider,
            IEventAggregator eventAggregator,
            ILoadingUnitsDataProvider loadingUnitsDataProvider,
            IMachineMissionsProvider missionsProvider,
            IMissionsDataProvider missionsDataProvider,
            ILogger<MissionSchedulingService> logger)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.cellsProvider = cellsProvider ?? throw new ArgumentNullException(nameof(cellsProvider));
            this.loadingUnitsDataProvider = loadingUnitsDataProvider ?? throw new ArgumentNullException(nameof(loadingUnitsDataProvider));
            this.machineMissionsProvider = missionsProvider ?? throw new ArgumentNullException(nameof(missionsProvider));
            this.notificationEvent = eventAggregator.GetEvent<NotificationEvent>();
            this.missionsDataProvider = missionsDataProvider ?? throw new ArgumentNullException(nameof(missionsDataProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #endregion

        #region Methods

        public void AbortMission(Mission mission)
        {
            if (mission is null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            this.missionsDataProvider.Complete(mission.Id);

            this.NotifyNewMachineMissionAvailable(mission);
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber)
        {
            this.logger.LogDebug(
                "Queuing local mission for loading unit {loadingUnitId} to bay {targetBayNumber}.",
                loadingUnitId,
                targetBayNumber);

            var mission = this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber);
            this.machineMissionsProvider.AddMission(mission, mission.FsmId);

            this.NotifyNewMachineMissionAvailable(mission);
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            this.logger.LogDebug(
                "Queuing WMS mission {wmsMissionId} for loading unit {loadingUnitId} to bay {targetBayNumber}.",
                wmsMissionId,
                loadingUnitId,
                targetBayNumber);

            var mission = this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, wmsMissionId, wmsMissionPriority);
            this.machineMissionsProvider.AddMission(mission, mission.FsmId);

            this.NotifyNewMachineMissionAvailable(mission);
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public void QueueLoadingUnitCompactingMission(IServiceProvider serviceProvider)
        {
            var loadUnits = this.loadingUnitsDataProvider.GetAll().Where(x => x.Cell != null);
            int? cellId;
            int? loadUnitId;
            // first we try to find a lower place for each load unit
            if (this.CompactFindEmptyCell(loadUnits, out loadUnitId, out cellId)
                // then we try to shift down the load units
                || this.CompactDownCell(loadUnits, out loadUnitId, out cellId)
                )
            {
                var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadingUnitProvider>();
                var loadUnit = loadUnits.First(x => x.Id == loadUnitId.Value);
                moveLoadingUnitProvider.MoveFromCellToCell(MissionType.Compact, loadUnit.Cell.Id, cellId, BayNumber.BayOne, MessageActor.MissionManager);
            }
            else
            {
                // no more compacting is possible. Exit from compact mode
                var machineModeDataProvider = serviceProvider.GetRequiredService<IMachineModeVolatileDataProvider>();
                machineModeDataProvider.Mode = MachineMode.Manual;
                this.logger.LogInformation($"Machine status switched to {machineModeDataProvider.Mode}");
            }
        }

        private bool CompactDownCell(IEnumerable<LoadingUnit> loadUnits, out int? loadUnitId, out int? cellId)
        {
            loadUnitId = null;
            cellId = null;
            if (!loadUnits.Any())
            {
                return false;
            }
            foreach (var loadUnit in loadUnits.OrderBy(o => o.Cell.Position))
            {
                try
                {
                    cellId = this.cellsProvider.FindDownCell(loadUnit);
                    loadUnitId = loadUnit.Id;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    // continue with next Load Unit
                }
            }
            return false;
        }

        private bool CompactFindEmptyCell(IEnumerable<LoadingUnit> loadUnits, out int? loadUnitId, out int? cellId)
        {
            loadUnitId = null;
            cellId = null;
            if (!loadUnits.Any())
            {
                return false;
            }
            foreach (var loadUnit in loadUnits.OrderByDescending(o => o.Cell.Position))
            {
                try
                {
                    cellId = this.cellsProvider.FindEmptyCell(loadUnit.Id, isCompacting: true);
                    loadUnitId = loadUnit.Id;
                    return true;
                }
                catch (InvalidOperationException)
                {
                    // continue with next Load Unit
                }
            }
            return false;
        }

        private void NotifyNewMachineMissionAvailable(Mission mission)
        {
            if (mission is null)
            {
                throw new ArgumentNullException(nameof(mission));
            }

            var notificationMessage = new NotificationMessage(
                null,
                $"New machine mission available for bay {mission.TargetBay}.",
                MessageActor.MissionManager,
                MessageActor.MissionManager,
                MessageType.NewMachineMissionAvailable,
                mission.TargetBay);

            this.notificationEvent.Publish(notificationMessage);
        }

        #endregion
    }
}
