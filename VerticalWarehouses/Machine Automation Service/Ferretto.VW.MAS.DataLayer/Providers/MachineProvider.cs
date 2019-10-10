using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class MachineProvider : IMachineProvider
    {
        #region Fields

        private const int MaxDrawerGrossWeight = 990;

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public MachineProvider(DataLayerContext dataContext)
        {
            if (dataContext == null)
            {
                throw new System.ArgumentNullException(nameof(dataContext));
            }

            this.dataContext = dataContext;
        }

        #endregion

        #region Methods

        public Machine Get()
        {
            return this.dataContext.Machines
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

        public double GetHeight()
        {
            return this.dataContext.Machines.Select(m => m.Height).Single();
        }

        public MachineStatistics GetStatistics()
        {
            return this.dataContext.MachineStatistics.FirstOrDefault();
        }

        public bool IsOneTonMachine()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.StructuralProperties)
                .Single();

            var maximumLoadOnBoard = elevator.StructuralProperties.MaximumLoadOnBoard;
            return maximumLoadOnBoard == MaxDrawerGrossWeight;
        }

        #endregion
    }
}
