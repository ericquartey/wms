using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly IMemoryCache cache;

        private readonly DataLayerContext dataContext;

        private readonly ILogger<MachineProvider> logger;

        #endregion

        #region Constructors

        public MachineProvider(
            DataLayerContext dataContext,
            ILogger<MachineProvider> logger,
            IMemoryCache cache)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        #endregion

        #region Properties

        public bool IsHomingExetuted { get; set; }

        public bool IsMachineRunning { get; set; }

        #endregion

        #region Methods

        public void Add(Machine machine)
        {
            if (machine is null)
            {
                throw new System.ArgumentNullException(nameof(machine));
            }

            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(BaysDataProvider.GetElevatorAxesCacheKey());

            lock (this.dataContext)
            {
                this.dataContext.Machines.Add(machine);
                this.dataContext.SaveChanges();
            }
        }

        public void ClearAll()
        {
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(BaysDataProvider.GetElevatorAxesCacheKey());

            lock (this.dataContext)
            {
                this.dataContext.Shutters.RemoveRange(this.dataContext.Shutters);
                this.dataContext.WeightMeasurements.RemoveRange(this.dataContext.WeightMeasurements);
                this.dataContext.Inverters.RemoveRange(this.dataContext.Inverters);
                this.dataContext.ElevatorStructuralProperties.RemoveRange(this.dataContext.ElevatorStructuralProperties);
                this.dataContext.LoadingUnits.RemoveRange(this.dataContext.LoadingUnits);
                this.dataContext.BayPositions.RemoveRange(this.dataContext.BayPositions);
                this.dataContext.CellPanels.RemoveRange(this.dataContext.CellPanels);
                this.dataContext.Cells.RemoveRange(this.dataContext.Cells);
                this.dataContext.IoDevices.RemoveRange(this.dataContext.IoDevices);
                this.dataContext.Bays.RemoveRange(this.dataContext.Bays);
                this.dataContext.MovementParameters.RemoveRange(this.dataContext.MovementParameters);
                this.dataContext.MovementProfiles.RemoveRange(this.dataContext.MovementProfiles);
                this.dataContext.ElevatorAxes.RemoveRange(this.dataContext.ElevatorAxes);
                this.dataContext.Elevators.RemoveRange(this.dataContext.Elevators);
                this.dataContext.Machines.RemoveRange(this.dataContext.Machines);
                this.dataContext.SetupProcedures.RemoveRange(this.dataContext.SetupProcedures);
                this.dataContext.SetupProceduresSets.RemoveRange(this.dataContext.SetupProceduresSets);
                this.dataContext.SaveChanges();
            }
        }

        public Machine Get()
        {
            lock (this.dataContext)
            {
                var entity =
                this.dataContext.Machines
                    .Include(m => m.Elevator)
                        .ThenInclude(e => e.Axes)
                            .ThenInclude(a => a.EmptyLoadMovement)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.FullLoadMovement)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.WeightMeasurement)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.AssistedMovements)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.ManualMovements)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.Profiles)
                               .ThenInclude(p => p.Steps)
                    .Include(m => m.Elevator)
                       .ThenInclude(e => e.Axes)
                           .ThenInclude(a => a.Inverter)
                    .Include(m => m.Elevator)
                        .ThenInclude(e => e.StructuralProperties)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Positions)
                            .ThenInclude(b => b.LoadingUnit)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Carousel)
                            .ThenInclude(b => b.ManualMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Carousel)
                            .ThenInclude(b => b.AssistedMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Inverter)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.IoDevice)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.AssistedMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.ManualMovements)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.Inverter)
                    .Include(m => m.Panels)
                        .ThenInclude(p => p.Cells)
                    .Single();

                entity.Elevator?.Axes?.ForEach((axe) =>
                {
                    axe.Profiles.ForEach((profile) => profile.Steps = profile.Steps.OrderBy(c => c.Number).ToList());
                });

                return entity;
            }
        }

        public double GetHeight()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Machines.Select(m => m.Height).Single();
            }
        }

        public MachineStatistics GetStatistics()
        {
            lock (this.dataContext)
            {
                return this.dataContext.MachineStatistics.FirstOrDefault();
            }
        }

        public void Import(Machine machine, DataLayerContext context)
        {
            _ = machine ?? throw new System.ArgumentNullException(nameof(machine));

            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(BaysDataProvider.GetElevatorAxesCacheKey());

            context.ElevatorAxisManualParameters.RemoveRange(context.ElevatorAxisManualParameters);
            context.ShutterManualParameters.RemoveRange(context.ShutterManualParameters);
            context.CarouselManualParameters.RemoveRange(context.CarouselManualParameters);
            context.Carousels.RemoveRange(context.Carousels);
            context.CellPanels.RemoveRange(context.CellPanels);
            context.Shutters.RemoveRange(context.Shutters);
            context.WeightMeasurements.RemoveRange(context.WeightMeasurements);
            context.Inverters.RemoveRange(context.Inverters);
            context.ElevatorStructuralProperties.RemoveRange(context.ElevatorStructuralProperties);
            context.BayPositions.RemoveRange(context.BayPositions);
            context.CellPanels.RemoveRange(context.CellPanels);
            context.Cells.RemoveRange(context.Cells);
            context.IoDevices.RemoveRange(context.IoDevices);
            context.Bays.RemoveRange(context.Bays);
            context.MovementParameters.RemoveRange(context.MovementParameters);
            context.MovementProfiles.RemoveRange(context.MovementProfiles);
            context.ElevatorAxes.RemoveRange(context.ElevatorAxes);
            context.Elevators.RemoveRange(context.Elevators);
            context.Machines.RemoveRange(context.Machines);

            context.Machines.Add(machine);
        }

        public bool IsOneTonMachine()
        {
            lock (this.dataContext)
            {
                var elevatorInvertersCount = this.dataContext.ElevatorAxes
                    .Where(a => a.Inverter != null)
                    .Select(a => a.Inverter.Id)
                    .Distinct()
                    .Count();

                return elevatorInvertersCount > 1;
            }
        }

        public void Update(Machine machine, DataLayerContext dataContext)
        {
            _ = machine ?? throw new System.ArgumentNullException(nameof(machine));

            dataContext = dataContext ?? this.dataContext;

            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(BaysDataProvider.GetElevatorAxesCacheKey());

            machine.Elevator?.Axes.ForEach((a) =>
            {
                dataContext.AddOrUpdate(a.EmptyLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(a.FullLoadMovement, (e) => e.Id);
                dataContext.AddOrUpdate(a.WeightMeasurement, (e) => e.Id);
                dataContext.AddOrUpdate(a.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(a.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(a.Inverter, (e) => e.Id);

                a.Profiles.ForEach((p) =>
                {
                    p.Steps.ForEach((s) => dataContext.AddOrUpdate(s, (e) => e.Id));
                    dataContext.AddOrUpdate(p, (e) => e.Id);
                });

                dataContext.AddOrUpdate(a, (e) => e.Id);
            });

            dataContext.AddOrUpdate(machine.Elevator?.StructuralProperties, (e) => e.Id);
            dataContext.AddOrUpdate(machine.Elevator, (e) => e.Id);

            machine.Bays.ForEach((b) =>
            {
                b.Positions.ForEach((p) =>
                {
                    dataContext.AddOrUpdate(p.LoadingUnit, (e) => e.Id);
                    dataContext.AddOrUpdate(p, (e) => e.Id);
                });

                dataContext.AddOrUpdate(b.Carousel, (e) => e.Id);
                dataContext.AddOrUpdate(b.Carousel?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Carousel?.ManualMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Inverter, (e) => e.Id);
                dataContext.AddOrUpdate(b.IoDevice, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.Inverter, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.AssistedMovements, (e) => e.Id);
                dataContext.AddOrUpdate(b.Shutter?.ManualMovements, (e) => e.Id);
            });

            machine.Panels.ForEach((p) =>
            {
                p.Cells.ForEach((c) => dataContext.AddOrUpdate(c, (e) => e.Id));
                dataContext.AddOrUpdate(p, (e) => e.Id);
            });

            dataContext.AddOrUpdate(machine, (e) => e.Id);

            dataContext.SaveChanges();
        }

        private void DeleteBays(IEnumerable<Bay> bays)
        {
            if (bays is null)
            {
                return;
            }

            foreach (var bay in bays)
            {
                bay.Inverter = null;
                bay.IoDevice = null;
                if (!(bay.Positions is null))
                {
                    foreach (var position in bay.Positions)
                    {
                        position.LoadingUnit = null;
                    }
                }
                bay.Positions = null;
                bay.Shutter = null;
            }
        }

        private void DeleteElevators(Elevator elevator)
        {
            if (elevator is null)
            {
                return;
            }
            if (!(elevator.Axes is null))
            {
                foreach (var axes in elevator.Axes)
                {
                    axes.FullLoadMovement = null;
                    axes.EmptyLoadMovement = null;
                    axes.Inverter = null;
                    foreach (var profile in axes.Profiles)
                    {
                        profile.Steps = null;
                    }
                    axes.WeightMeasurement = null;
                }
            }
            elevator.StructuralProperties = null;
        }

        #endregion
    }
}
