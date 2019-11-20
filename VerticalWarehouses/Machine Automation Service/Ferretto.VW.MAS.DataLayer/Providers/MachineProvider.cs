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

        #region Methods

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
                //this.dataContext.Database.ExecuteSqlCommand("PRAGMA foreign_keys = OFF");
                //this.dataContext.Database.ExecuteSqlCommand("PRAGMA ignore_check_constraints = ON;");
                // this.dataContext.SaveChanges();
                foreach (var mac in this.dataContext.Machines)
                {
                    this.DeleteBays(mac.Bays);

                    this.DeleteElevators(mac.Elevator);

                    if (!(mac.Panels is null))
                    {
                        foreach (var panel in mac.Panels)
                        {
                            if (!(panel.Cells is null))
                            {
                                foreach (var cell in panel.Cells)
                                {
                                    cell.LoadingUnit = null;
                                    cell.Panel = null;
                                }
                                panel.Cells = null;
                            }
                        }
                    }
                }

                this.DeleteBays(this.dataContext.Bays);

                this.dataContext.LoadingUnits.RemoveRange(this.dataContext.LoadingUnits);

                this.dataContext.Bays.ForEachAsync(b => b.Positions = null);
                this.dataContext.BayPositions.RemoveRange(this.dataContext.BayPositions);

                this.dataContext.Cells.ForEachAsync(c => c.Panel = null);
                this.dataContext.CellPanels.RemoveRange(this.dataContext.CellPanels);
                this.dataContext.Cells.RemoveRange(this.dataContext.Cells);

                //this.dataContext.Bays.ForEachAsync(b => b.IoDevice = null);
                this.dataContext.IoDevices.RemoveRange(this.dataContext.IoDevices);

                this.dataContext.Bays.ForEachAsync(b => b.Inverter = null);
                
                this.dataContext.Inverters.RemoveRange(this.dataContext.Inverters);

                this.DeleteBays(this.dataContext.Bays);

                this.dataContext.Bays.ForEachAsync(b => b.Shutter = null);
                this.dataContext.Bays.RemoveRange(this.dataContext.Bays);

                this.dataContext.Bays.RemoveRange(this.dataContext.Bays);

                this.dataContext.MovementParameters.RemoveRange(this.dataContext.MovementParameters);
                this.dataContext.MovementProfiles.RemoveRange(this.dataContext.MovementProfiles);

                //this.dataContext.Machines.ForEachAsync(m => m.Elevator.Axes = null);

                this.dataContext.ElevatorAxes.RemoveRange(this.dataContext.ElevatorAxes);

                //this.dataContext.Machines.ForEachAsync(m => m.Elevator.StructuralProperties = null);
                this.dataContext.ElevatorStructuralProperties.RemoveRange(this.dataContext.ElevatorStructuralProperties);

                this.dataContext.Elevators.ForEachAsync(e => this.DeleteElevators(e));
                this.dataContext.Elevators.RemoveRange(this.dataContext.Elevators);

                this.dataContext.Cells.RemoveRange(this.dataContext.Cells);
                this.dataContext.CellPanels.RemoveRange(this.dataContext.CellPanels);

                //this.dataContext.Carousels.RemoveRange(this.dataContext.Carousels);
                //this.dataContext.ErrorDefinitions.RemoveRange(this.dataContext.ErrorDefinitions);
                //this.dataContext.Errors.RemoveRange(this.dataContext.Errors);
                //this.dataContext.ErrorStatistics.RemoveRange(this.dataContext.ErrorStatistics);
                //this.dataContext.LoadingUnits.RemoveRange(this.dataContext.LoadingUnits);
                //this.dataContext.LogEntries.RemoveRange(this.dataContext.LogEntries);
                //this.dataContext.MachineStatistics.RemoveRange(this.dataContext.MachineStatistics);
                //this.dataContext.ServicingInfo.RemoveRange(this.dataContext.ServicingInfo);
                //this.dataContext.SetupProcedures.RemoveRange(this.dataContext.SetupProcedures);
                //this.dataContext.SetupProceduresSets.RemoveRange(this.dataContext.SetupProceduresSets);
                //this.dataContext.SetupStatus.RemoveRange(this.dataContext.SetupStatus);
                //this.dataContext.TorqueCurrentMeasurementSessions.RemoveRange(this.dataContext.TorqueCurrentMeasurementSessions);
                //this.dataContext.TorqueCurrentSamples.RemoveRange(this.dataContext.TorqueCurrentSamples);
                //this.dataContext.Users.RemoveRange(this.dataContext.Users);

                this.dataContext.Machines.RemoveRange(this.dataContext.Machines);

                this.dataContext.SaveChanges();

                //this.dataContext.Machines.Update(machine);

                //this.dataContext.Database.ExecuteSqlCommand("PRAGMA foreign_keys = ON");
                //this.dataContext.Database.ExecuteSqlCommand("PRAGMA ignore_check_constraints = ON;");

                //this.dataContext.SaveChanges();
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
                bay.IoDeviceId = null;
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
