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

        /// <summary>
        /// Computes the elevator displacement due to belt and shaft mechanical distorsions.
        /// </summary>
        /// <param name="targetPosition">The vertical position of the elevator, in millimeters.</param>
        /// <returns>The vertical position displacement, in millimeters.</returns>
        public double ComputeDisplacement(double targetPosition)
        {
            var loadingUnit = this.GetLoadingUnitOnBoard();
            if (loadingUnit is null)
            {
                return 0;
            }

            var shaftTorsion = this.ComputeShaftTorsion(loadingUnit.GrossWeight);
            var beltElongation = this.ComputeBeltElongation(loadingUnit.GrossWeight, targetPosition);

            return beltElongation + shaftTorsion;
        }

        public int ConvertMillimetersToPulses(double millimeters, Orientation orientation)
        {
            var axis = this.GetAxis(orientation);

            return (int)(axis.Resolution * (decimal)millimeters);
        }

        public double ConvertPulsesToMillimeters(int pulses, Orientation orientation)
        {
            if (pulses == 0)
            {
                throw new ArgumentOutOfRangeException("Pulses must be different from zero.", nameof(pulses));
            }

            var axis = this.GetAxis(orientation);

            if (axis.Resolution == 0)
            {
                throw new InvalidOperationException(
                    $"Configured {orientation} axis resolution is zero, therefore it is not possible to convert pulses to millimeters.");
            }

            return (double)(pulses / axis.Resolution);
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

        public void SetLoadingUnitOnBoard(int? id)
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.LoadingUnit)
                .Single();

            elevator.LoadingUnitId = id;

            this.dataContext.SaveChanges();
        }


        public double GetMaximumLoadOnBoard()
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.StructuralProperties)
                .Single();

            return elevator.StructuralProperties.MaximumLoadOnBoard;
        }

        public ElevatorAxis GetVerticalAxis()
        {
            return this.GetAxis(Orientation.Vertical);
        }

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

        public void IncreaseDepositAndPickUpCycleQuantity()
        {
            var horizontalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Horizontal);

            horizontalAxis.TotalCycles++;

            this.dataContext.SaveChanges();
        }

        public void ResetDepositAndPickUpCycleQuantity()
        {
            var horizontalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Horizontal);

            horizontalAxis.TotalCycles = 0;

            this.dataContext.SaveChanges();
        }


        /// <summary>
        /// Computes the vertical position displacement due to the belt elongation, due to the given loading unit weight.
        /// </summary>
        /// <param name="grossWeight">The gross weight loaded on the elevator, in kilograms.</param>
        /// <param name="targetPosition">The vertical position of the elevator, in millimeters.</param>
        /// <returns>The vertical position displacement, in millimeters.</returns>
        private double ComputeBeltElongation(double grossWeight, double targetPosition)
        {
            var machineHeight = this.dataContext.Machines.Single().Height;

            var pulleysDistanceMeters = (machineHeight - ElevatorStructuralProperties.PulleysMargin) / 1000;

            var properties = this.dataContext.ElevatorStructuralProperties.Single();

            var beltSpacingMeters = properties.BeltSpacing / 1000;

            var targetPositionMeters = targetPosition / 1000;

            return
                5000 * grossWeight
                /
                ((properties.BeltRigidity / ((2 * pulleysDistanceMeters) - beltSpacingMeters - targetPositionMeters)) + (properties.BeltRigidity / targetPositionMeters));
        }

        /// <summary>
        /// Computes the vertical position displacement due to the shaft torsion.
        /// </summary>
        /// <param name="grossWeight">The gross weight loaded on the elevator, in kilograms.</param>
        /// <returns>The vertical position displacement, in millimeters.</returns>
        private double ComputeShaftTorsion(double grossWeight)
        {
            var properties = this.dataContext.ElevatorStructuralProperties.Single();

            const double m = 10.0 / 3;

            return
                64 * (m + 1) * (grossWeight * Math.Pow(properties.PulleyDiameter, 2) * properties.HalfShaftLength)
                /
                (Math.PI * Math.Pow(properties.ShaftDiameter, 4) * m * properties.ShaftElasticity);
        }

        private ElevatorAxis GetAxis(Orientation orientation)
        {
            return this.dataContext.ElevatorAxes
                .Include(a => a.Profiles)
                .ThenInclude(p => p.Steps)
                .Include(a => a.MaximumLoadMovement)
                .Include(a => a.EmptyLoadMovement)
                .Single(a => a.Orientation == orientation);
        }

        #endregion
    }
}
