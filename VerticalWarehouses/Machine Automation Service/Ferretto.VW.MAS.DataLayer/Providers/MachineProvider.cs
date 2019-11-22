using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private readonly IMemoryCache cache;

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public MachineProvider(
            DataLayerContext dataContext,
            IMemoryCache cache)
        {
            this.dataContext = dataContext ?? throw new System.ArgumentNullException(nameof(dataContext));
            this.cache = cache ?? throw new System.ArgumentNullException(nameof(cache));
        }

        #endregion

        #region Properties

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
            this.cache.Remove(BaysProvider.GetElevatorAxesCacheKey());

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
            this.cache.Remove(BaysProvider.GetElevatorAxesCacheKey());

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
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Inverter)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.IoDevice)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                            .ThenInclude(b => b.Inverter)
                    .Include(m => m.Panels)
                        .ThenInclude(p => p.Cells)
                    .Single();

                foreach (var axe in entity.Elevator.Axes.ToList())
                {
                    foreach (var profile in axe.Profiles.ToList())
                    {
                        profile.Steps = profile.Steps.OrderBy(c => c.Number).ToList();
                    }
                }

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

        public void Update(Machine machine)
        {
            if (machine is null)
            {
                throw new System.ArgumentNullException(nameof(machine));
            }

            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Vertical));
            this.cache.Remove(ElevatorDataProvider.GetAxisCacheKey(Orientation.Horizontal));
            this.cache.Remove(BaysProvider.GetElevatorAxesCacheKey());

            lock (this.dataContext)
            {
                if (!(machine.Elevator.Axes is null))
                {
                    foreach (var a in machine.Elevator.Axes)
                    {
                        if (!(a.EmptyLoadMovement is null))
                        {
                            this.dataContext.MovementParameters.Attach(a.EmptyLoadMovement);
                            this.dataContext.Entry(a.EmptyLoadMovement).State = EntityState.Modified;
                            this.dataContext.MovementParameters.Update(a.EmptyLoadMovement);
                        }

                        if (!(a.FullLoadMovement is null))
                        {
                            this.dataContext.MovementParameters.Attach(a.FullLoadMovement);
                            this.dataContext.Entry(a.FullLoadMovement).State = EntityState.Modified;
                            this.dataContext.MovementParameters.Update(a.FullLoadMovement);
                        }

                        if (!(a.WeightMeasurement is null))
                        {
                            this.dataContext.WeightMeasurements.Attach(a.WeightMeasurement);
                            this.dataContext.Entry(a.WeightMeasurement).State = EntityState.Modified;
                            this.dataContext.WeightMeasurements.Update(a.WeightMeasurement);
                        }

                        if (!(a.Profiles is null))
                        {
                            foreach (var p in a.Profiles)
                            {
                                if (!(p.Steps is null))
                                {
                                    foreach (var s in p.Steps)
                                    {
                                        this.dataContext.MovementParameters.Attach(s);
                                        this.dataContext.Entry(s).State = EntityState.Modified;
                                        this.dataContext.MovementParameters.Update(s);
                                    }
                                }

                                this.dataContext.MovementProfiles.Attach(p);
                                this.dataContext.Entry(p).State = EntityState.Modified;
                                this.dataContext.MovementProfiles.Update(p);
                            }
                        }

                        if (!(a.Inverter is null))
                        {
                            this.dataContext.Inverters.Attach(a.Inverter);
                            this.dataContext.Entry(a.Inverter).State = EntityState.Modified;
                            this.dataContext.Inverters.Update(a.Inverter);
                        }
                        this.dataContext.ElevatorAxes.Attach(a);
                        this.dataContext.Entry(a).State = EntityState.Modified;
                        this.dataContext.ElevatorAxes.Update(a);
                    }
                }

                if (!(machine.Elevator.StructuralProperties is null))
                {
                    this.dataContext.ElevatorStructuralProperties.Attach(machine.Elevator.StructuralProperties);
                    this.dataContext.Entry(machine.Elevator.StructuralProperties).State = EntityState.Modified;
                    this.dataContext.ElevatorStructuralProperties.Update(machine.Elevator.StructuralProperties);
                }

                if (!(machine.Elevator is null))
                {
                    this.dataContext.Elevators.Attach(machine.Elevator);
                    this.dataContext.Entry(machine.Elevator).State = EntityState.Modified;
                    this.dataContext.Elevators.Update(machine.Elevator);
                }

                if (!(machine.Bays is null))
                {
                    foreach (var b in machine.Bays)
                    {
                        if (!(b.Positions is null))
                        {
                            foreach (var p in b.Positions)
                            {
                                if (!(p.LoadingUnit is null))
                                {
                                    this.dataContext.LoadingUnits.Attach(p.LoadingUnit);
                                    this.dataContext.Entry(p.LoadingUnit).State = EntityState.Modified;
                                    this.dataContext.LoadingUnits.Update(p.LoadingUnit);
                                }
                                this.dataContext.BayPositions.Attach(p);
                                this.dataContext.Entry(p).State = EntityState.Modified;
                                this.dataContext.BayPositions.Update(p);
                            }
                        }

                        if (!(b.Carousel is null))
                        {
                            this.dataContext.Carousels.Attach(b.Carousel);
                            this.dataContext.Entry(b.Carousel).State = EntityState.Modified;
                            this.dataContext.Carousels.Update(b.Carousel);
                        }

                        if (!(b.Inverter is null))
                        {
                            this.dataContext.Inverters.Attach(b.Inverter);
                            this.dataContext.Entry(b.Inverter).State = EntityState.Modified;
                            this.dataContext.Inverters.Update(b.Inverter);
                        }

                        if (!(b.IoDevice is null))
                        {
                            this.dataContext.IoDevices.Attach(b.IoDevice);
                            this.dataContext.Entry(b.IoDevice).State = EntityState.Modified;
                            this.dataContext.IoDevices.Update(b.IoDevice);
                        }

                        if (!(b.Shutter is null))
                        {
                            this.dataContext.Shutters.Attach(b.Shutter);
                            this.dataContext.Entry(b.Shutter).State = EntityState.Modified;
                            this.dataContext.Shutters.Update(b.Shutter);
                        }

                        if (!(b.Shutter.Inverter is null))
                        {
                            this.dataContext.Inverters.Attach(b.Shutter.Inverter);
                            this.dataContext.Entry(b.Shutter.Inverter).State = EntityState.Modified;
                            this.dataContext.Inverters.Update(b.Shutter.Inverter);
                        }
                    }
                }

                if (!(machine.Panels is null))
                {
                    foreach (var p in machine.Panels)
                    {
                        if (!(p.Cells is null))
                        {
                            foreach (var c in p.Cells)
                            {
                                this.dataContext.Cells.Attach(c);
                                this.dataContext.Entry(c).State = EntityState.Modified;
                                this.dataContext.Cells.Update(c);
                            }
                        }

                        this.dataContext.CellPanels.Attach(p);
                        this.dataContext.Entry(p).State = EntityState.Modified;
                        this.dataContext.CellPanels.Update(p);
                    }
                }

                this.dataContext.Machines.Attach(machine);
                this.dataContext.Entry(machine).State = EntityState.Modified;
                this.dataContext.Machines.Update(machine);

                this.dataContext.SaveChanges();
            }
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
