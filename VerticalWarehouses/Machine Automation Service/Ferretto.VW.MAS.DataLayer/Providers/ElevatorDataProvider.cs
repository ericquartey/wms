using System;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
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

        public int ConvertMillimetersToPulses(decimal millimeters, Orientation orientation)
        {
            return (int)(this.GetAxis(orientation).Resolution * millimeters);
        }

        public decimal ConvertPulsesToMillimeters(int pulses, Orientation orientation)
        {
            if (pulses == 0)
            {
                throw new ArgumentOutOfRangeException("Pulses must be different from zero.", nameof(pulses));
            }

            var resolution = this.GetAxis(orientation).Resolution;

            if (resolution == 0)
            {
                throw new InvalidOperationException(
                    $"Configured {orientation} axis resolution is zero, therefore it is not possible to convert pulses to millimeters.");
            }

            return pulses / resolution;
        }

        public ElevatorAxis GetHorizontalAxis()
        {
            return this.GetAxis(Orientation.Horizontal);
        }

        public LoadingUnit GetLoadingUnitOnBoard()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.LoadingUnit)
                .ThenInclude(l => l.Cell)
                .Single();

            return elevator.LoadingUnit;
        }

        public decimal GetMaximumLoadOnBoard()
        {
            var elevator = this.dataContext.Elevators.Single();

            return elevator.StructuralProperties.MaximumLoadOnBoard;
        }

        public ElevatorAxis GetVerticalAxis()
        {
            return this.GetAxis(Orientation.Vertical);
        }

        public void UpdateVerticalOffset(decimal newOffset)
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

        private ElevatorAxis GetAxis(Orientation orientation)
        {
            return this.dataContext.ElevatorAxes
                .Include(a => a.Profiles)
                .Include(a => a.MaximumLoadMovement)
                .Include(a => a.EmptyLoadMovement)
                .Single(a => a.Orientation == orientation);
        }

        #endregion
    }
}
