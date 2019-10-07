using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ElevatorDataProvider : IElevatorDataProvider
    {
        #region Fields

        private readonly IDictionary<Orientation, ElevatorAxis> cachedAxes = new Dictionary<Orientation, ElevatorAxis>();

        private readonly DataLayerContext dataContext;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public ElevatorDataProvider(DataLayerContext dataContext, ISetupStatusProvider setupStatusProvider)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
        }

        #endregion

        #region Methods

        public ElevatorAxis GetAxis(Orientation orientation)
        {
            if (!this.cachedAxes.ContainsKey(orientation))
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

                this.cachedAxes.Add(orientation, axis);
            }

            return this.cachedAxes[orientation];
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
            this.cachedAxes[Orientation.Vertical] = verticalAxis;

            verticalAxis.Offset = newOffset;

            this.dataContext.ElevatorAxes.Update(verticalAxis);

            this.dataContext.SaveChanges();
        }

        public void UpdateVerticalResolution(decimal newResolution)
        {
            var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);
            this.cachedAxes[Orientation.Vertical] = verticalAxis;

            verticalAxis.Resolution = newResolution;
            this.dataContext.SaveChanges();

            this.setupStatusProvider.CompleteVerticalResolution();
        }

        #endregion
    }
}
