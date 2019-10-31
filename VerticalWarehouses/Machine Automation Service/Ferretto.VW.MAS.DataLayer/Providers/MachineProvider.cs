using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private const int MaxDrawerGrossWeight = 990;

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
                return this.dataContext.Machines
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
                        .ThenInclude(e => e.StructuralProperties)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Positions)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Carousel)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Inverter)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.IoDevice)
                    .Include(m => m.Bays)
                        .ThenInclude(b => b.Shutter)
                    .Include(m => m.Panels)
                        .ThenInclude(p => p.Cells)
                    .Single();
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
                var elevator = this.dataContext.Elevators
                    .Include(e => e.StructuralProperties)
                    .Single();

                var maximumLoadOnBoard = elevator.StructuralProperties.MaximumLoadOnBoard;
                return maximumLoadOnBoard == MaxDrawerGrossWeight;
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

            lock (this.dataContext)
            {
                this.dataContext.Machines.Update(machine);

                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
