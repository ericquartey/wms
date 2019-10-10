using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.MAS.DataLayer.DatabaseContext;
using Ferretto.VW.MAS.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class ElevatorDataProvider : IElevatorDataProvider
    {
        #region Fields

        private readonly IDictionary<Orientation, ElevatorAxis> cachedAxes = new Dictionary<Orientation, ElevatorAxis>();

        private readonly DataLayerContext dataContext;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public ElevatorDataProvider(
            DataLayerContext dataContext,
            ISetupStatusProvider setupStatusProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
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

        public void IncreaseCycleQuantity(Orientation orientation)
        {
            var axis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == orientation);
            if (axis is null)
            {
                throw new EntityNotFoundException(orientation.ToString());
            }

            axis.TotalCycles++;

            this.dataContext.SaveChanges();
        }

        public void SetLoadingUnitOnBoard(int? id)
        {
            var elevator = this.dataContext.Elevators
                .Include(e => e.LoadingUnit)
                .Single();

            elevator.LoadingUnitId = id;

            this.dataContext.SaveChanges();
        }

        public void UpdateVerticalOffset(double newOffset)
        {
            var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);
            this.cachedAxes[Orientation.Vertical] = verticalAxis;

            verticalAxis.Offset = newOffset;
            this.dataContext.ElevatorAxes.Update(verticalAxis);
            this.dataContext.SaveChanges();

            var procedureParameters = this.setupProceduresDataProvider.GetAll().OffsetCalibration;
            this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            this.setupStatusProvider.CompleteVerticalOffset();
        }

        public void UpdateVerticalResolution(decimal newResolution)
        {
            var verticalAxis = this.dataContext.ElevatorAxes.SingleOrDefault(a => a.Orientation == Orientation.Vertical);
            this.cachedAxes[Orientation.Vertical] = verticalAxis;

            verticalAxis.Resolution = newResolution;
            this.dataContext.ElevatorAxes.Update(verticalAxis);
            this.dataContext.SaveChanges();

            var procedureParameters = this.setupProceduresDataProvider.GetAll().VerticalResolutionCalibration;
            this.setupProceduresDataProvider.MarkAsCompleted(procedureParameters);
            this.setupStatusProvider.CompleteVerticalResolution();
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

        #endregion
    }
}
