using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer.Providers
{
    internal sealed class ElevatorDataProvider : IElevatorDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public ElevatorDataProvider(DataLayerContext dataContext, ISetupStatusProvider setupStatusProvider)
        {
            if (dataContext is null)
            {
                throw new ArgumentNullException(nameof(dataContext));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.dataContext = dataContext;
            this.setupStatusProvider = setupStatusProvider;
        }

        #endregion

        #region Methods

        public ElevatorAxis GetAxis(Orientation orientation)
        {
            var axis = this.dataContext.ElevatorAxes
                .Include(a => a.Profiles)
                .ThenInclude(p => p.Steps)
                .Include(a => a.MaximumLoadMovement)
                .Include(a => a.EmptyLoadMovement)
                .SingleOrDefault(a => a.Orientation == orientation);

            if (axis is null)
            {
                throw new EntityNotFoundException(orientation.ToString());
            }

            return axis;
        }

        public ElevatorAxis GetHorizontalAxis() => this.GetAxis(Orientation.Horizontal);

        public LoadingUnit GetLoadingUnitOnBoard()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .Single();

            return elevator.LoadingUnit;
        }

        public double GetMaximumLoadOnBoard()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.StructuralProperties)
                .Single();

            return elevator.StructuralProperties.MaximumLoadOnBoard;
        }

        public ElevatorStructuralProperties GetStructuralProperties()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.StructuralProperties)
                .Single();

            return elevator.StructuralProperties;
        }

        public ElevatorAxis GetVerticalAxis() => this.GetAxis(Orientation.Vertical);

        public void UpdateVerticalOffset(double newOffset)
        {
            var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);

            verticalAxis.Offset = newOffset;

            this.dataContext.ElevatorAxes.Update(verticalAxis);

            this.dataContext.SaveChanges();
        }

        public void UpdateVerticalResolution(decimal newResolution)
        {
            var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);

            verticalAxis.Resolution = newResolution;
            this.dataContext.SaveChanges();

            this.setupStatusProvider.CompleteVerticalResolution();
        }

        #endregion
    }
}
