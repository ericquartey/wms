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
using Ferretto.VW.MAS.Utils.Enumerations;

namespace Ferretto.VW.MAS.MissionManager
{
    internal sealed class MissionSchedulingProvider : IMissionSchedulingProvider
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

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
            IBaysDataProvider baysDataProvider,
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
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
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

            this.NotifyNewMachineMissionAvailable(mission.TargetBay);
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, MissionType missionType)
        {
            this.logger.LogDebug(
                $"Queuing local mission for load unit {loadingUnitId} to bay {targetBayNumber}.",
                loadingUnitId,
                targetBayNumber);

            var mission = this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, missionType);

            if (mission != null)
            {
                this.NotifyNewMachineMissionAvailable(mission.TargetBay);
            }
        }

        public void QueueBayMission(int loadingUnitId, BayNumber targetBayNumber, int wmsMissionId, int wmsMissionPriority)
        {
            this.logger.LogDebug(
                "Queuing WMS mission {wmsMissionId} for loading unit {loadingUnitId} to bay {targetBayNumber}.",
                wmsMissionId,
                loadingUnitId,
                targetBayNumber);

            var loadingUnit = this.loadingUnitsDataProvider.GetById(loadingUnitId);
            if (!loadingUnit.IsIntoMachine)
            {
                throw new InvalidOperationException($"The loading unit {loadingUnitId} is not contained in the machine.");
            }

            var mission = this.missionsDataProvider.CreateBayMission(loadingUnitId, targetBayNumber, wmsMissionId, wmsMissionPriority);

            this.NotifyNewMachineMissionAvailable(mission.TargetBay);
        }

        public void QueueCellMission(int loadingUnitId, int targetCellId)
        {
            throw new NotImplementedException();
        }

        public bool QueueFirstTestMission(int loadUnitId, BayNumber sourceBayNumber, int cycle, IServiceProvider serviceProvider)
        {
            try
            {
                var cellId = this.cellsProvider.FindEmptyCell(loadUnitId, CompactingType.NoCompacting, isCellTest: true);
                var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();

                if (cycle > 0)
                {
                    var loadUnit = this.loadingUnitsDataProvider.GetById(loadUnitId);
                    this.logger.LogInformation($"Move from cell {loadUnit.CellId} to cell {cellId} First test");
                    moveLoadingUnitProvider.MoveFromCellToCell(MissionType.FirstTest, loadUnit.CellId, cellId, sourceBayNumber, MessageActor.MissionManager);
                    return true;
                }
                // first loop: load from bay
                var baysDataProvider = serviceProvider.GetRequiredService<IBaysDataProvider>();
                var loadUnitSource = baysDataProvider.GetLoadingUnitLocationByLoadingUnit(loadUnitId);
                if (loadUnitSource == LoadingUnitLocation.NoLocation)
                {
                    loadUnitSource = baysDataProvider.GetByNumber(sourceBayNumber).Positions.OrderBy(p => p.IsUpper).LastOrDefault().Location;
                }
                this.logger.LogInformation($"Move from bay {sourceBayNumber} to cell {cellId} First test");
                moveLoadingUnitProvider.InsertToCell(MissionType.FirstTest, loadUnitSource, cellId, loadUnitId, sourceBayNumber, MessageActor.MissionManager);
                return true;
            }
            catch (InvalidOperationException)
            {
                // no more testing is possible. Exit from test mode
                //this.logger.LogError(e, e.Message);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, ex.Message);
            }
            return false;
        }

        /// <summary>
        /// Search for a single compacting mission and activate it.
        /// This method will be repeated after each mission has finished when machine is in Compact mode.
        /// When no compacting mission is available machine returns to manual mode
        /// </summary>
        /// <param name="serviceProvider"></param>
        public bool QueueLoadingUnitCompactingMission(IServiceProvider serviceProvider)
        {
            var loadUnits = this.loadingUnitsDataProvider.GetAllCompacting()
                .OrderByDescending(l => l.Cell.Position)
                .ToList();
            int? cellId;
            LoadingUnit loadUnit;

            this.cellsProvider.CleanUnderWeightCells();
            // first we try to find a lower place for each load unit, matching exactly the height
            if (this.CompactFindEmptyCell(loadUnits, CompactingType.ExactMatchCompacting, out loadUnit, out cellId)
                // then we try to find a lower place for each load unit
                || this.CompactFindEmptyCell(loadUnits, CompactingType.AnySpaceCompacting, out loadUnit, out cellId)
                // then we try to shift down the load units
                || this.CompactDownCell(loadUnits, out loadUnit, out cellId)
                )
            {
                var moveLoadingUnitProvider = serviceProvider.GetRequiredService<IMoveLoadUnitProvider>();
                this.logger.LogInformation($"Move from cell {loadUnit.Cell.Id} to cell {cellId} Compact");
                moveLoadingUnitProvider.MoveFromCellToCell(MissionType.Compact, loadUnit.Cell.Id, cellId, BayNumber.BayOne, MessageActor.MissionManager);
                return true;
            }
            // no more compacting is possible. Exit from compact mode
            return false;
        }

        public void QueueRecallMission(int loadingUnitId, BayNumber sourceBayNumber, MissionType missionType)
        {
            var bay = this.baysDataProvider.GetByNumber(sourceBayNumber);
            if (bay.Positions.Any(x => x.LoadingUnit?.Id == loadingUnitId))
            {
                this.logger.LogDebug(
                  "Queuing local recall mission for loading unit {loadingUnitId} from bay {sourceBayNumber}.",
                  loadingUnitId,
                  sourceBayNumber);
                var mission = this.missionsDataProvider.CreateRecallMission(loadingUnitId, sourceBayNumber, missionType);

                this.NotifyNewMachineMissionAvailable(mission.TargetBay);
            }
            else
            {
                this.logger.LogError(
                  "Load unit {loadingUnitId} not in bay {sourceBayNumber}",
                  loadingUnitId,
                  sourceBayNumber);
            }
        }

        private bool CompactDownCell(IEnumerable<LoadingUnit> loadUnits, out LoadingUnit loadUnitOut, out int? cellId)
        {
            loadUnitOut = null;
            cellId = null;
            if (!loadUnits.Any())
            {
                return false;
            }
            this.logger.LogDebug("Compacting down cells");
            foreach (var loadUnit in loadUnits.OrderBy(o => o.Cell.Position))
            {
                try
                {
                    if (loadUnit.IsIntoMachine)
                    {
                        cellId = this.cellsProvider.FindDownCell(loadUnit);
                        loadUnitOut = loadUnit;
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // continue with next Load Unit
                }
            }
            return false;
        }

        private bool CompactFindEmptyCell(List<LoadingUnit> loadUnits, CompactingType compactingType, out LoadingUnit loadUnitOut, out int? cellId)
        {
            loadUnitOut = null;
            cellId = null;
            if (!loadUnits.Any())
            {
                return false;
            }
            this.logger.LogDebug($"Compacting empty cells {compactingType}");
            foreach (var loadUnit in loadUnits)
            {
                try
                {
                    if (loadUnit.IsIntoMachine)
                    {
                        cellId = this.cellsProvider.FindEmptyCell(loadUnit.Id, compactingType);
                        loadUnitOut = loadUnit;
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // continue with next Load Unit
                }
            }
            return false;
        }

        private void NotifyNewMachineMissionAvailable(BayNumber bay)
        {
            this.logger.LogDebug($"Notify a new machine mission available!");

            var notificationMessage = new NotificationMessage(
                null,
                $"New machine mission available for bay {bay}.",
                MessageActor.MissionManager,
                MessageActor.MissionManager,
                MessageType.NewMachineMissionAvailable,
                bay);

            this.notificationEvent.Publish(notificationMessage);
        }

        #endregion
    }
}
