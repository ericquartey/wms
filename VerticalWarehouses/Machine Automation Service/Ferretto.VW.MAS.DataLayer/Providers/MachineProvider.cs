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
                this.dataContext.Machines.RemoveRange(this.dataContext.Machines);

                this.dataContext.Machines.Update(machine);

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
