using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver
{
    internal class InvertersProvider : IInvertersProvider
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IMachineProvider machineProvider;

        private IEnumerable<IInverterStatusBase> inverters;

        #endregion

        #region Constructors

        public InvertersProvider(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IMachineProvider machineProvider,
            IBaysProvider baysProvider,
            IDigitalDevicesDataProvider digitalDevicesDataProvider)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.baysProvider = baysProvider ?? throw new ArgumentNullException(nameof(baysProvider));
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));

            eventAggregator
                .GetEvent<NotificationEvent>()
                .Subscribe(
                    m => this.OnDataLayerReady(),
                    ThreadOption.PublisherThread,
                    false,
                    message => message.Type == CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);

            try
            {
                this.OnDataLayerReady();
            }
            catch
            {
                // do nothing
            }
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
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
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
            var axis = this.elevatorDataProvider.GetAxis(orientation);

            return (int)(axis.Resolution * (decimal)millimeters);
        }

        public double ConvertPulsesToMillimeters(int pulses, Orientation orientation)
        {
            if (pulses == 0)
            {
                throw new ArgumentOutOfRangeException("Pulses must be different from zero.", nameof(pulses));
            }

            var axis = this.elevatorDataProvider.GetAxis(orientation);

            if (axis.Resolution == 0)
            {
                throw new InvalidOperationException(
                    $"Configured {orientation} axis resolution is zero, therefore it is not possible to convert pulses to millimeters.");
            }

            return (double)(pulses / axis.Resolution);
        }

        public IEnumerable<IInverterStatusBase> GetAll()
        {
            if (this.inverters is null)
            {
                throw new InvalidOperationException("The inverter configuration is not yet loaded because data layer is not ready.");
            }

            return this.inverters;
        }

        public IInverterStatusBase GetByIndex(InverterIndex index)
        {
            var inverter = this.inverters.SingleOrDefault(i => i.SystemIndex == index);

            if (inverter is null)
            {
                throw new EntityNotFoundException(index.ToString());
            }

            return inverter;
        }

        public IAngInverterStatus GetMainInverter()
        {
            System.Diagnostics.Debug.Assert(this.inverters.Any(i => i.SystemIndex == InverterIndex.MainInverter));

            return this.inverters.Single(i => i.SystemIndex == InverterIndex.MainInverter) as IAngInverterStatus;
        }

        public IInverterStatusBase GetShutterInverter(BayNumber bayNumber)
        {
            var index = this.baysProvider.GetByNumber(bayNumber).Shutter.Inverter.Index;
            var inverter = this.inverters.SingleOrDefault(i => i.SystemIndex == index);

            if (inverter is null)
            {
                throw new EntityNotFoundException(index.ToString());
            }

            return inverter;
        }

        /// <summary>
        /// Computes the vertical position displacement due to the belt elongation, due to the given loading unit weight.
        /// </summary>
        /// <param name="grossWeight">The gross weight loaded on the elevator, in kilograms.</param>
        /// <param name="targetPosition">The vertical position of the elevator, in millimeters.</param>
        /// <returns>The vertical position displacement, in millimeters.</returns>
        private double ComputeBeltElongation(double grossWeight, double targetPosition)
        {
            var machineHeight = this.machineProvider.GetHeight();

            var pulleysDistanceMeters = (machineHeight - ElevatorStructuralProperties.PulleysMargin) / 1000;

            var properties = this.elevatorDataProvider.GetStructuralProperties();

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
            var properties = this.elevatorDataProvider.GetStructuralProperties();

            const double m = 10.0 / 3;

            return
                64 * (m + 1) * (grossWeight * Math.Pow(properties.PulleyDiameter, 2) * properties.HalfShaftLength)
                /
                (Math.PI * Math.Pow(properties.ShaftDiameter, 4) * m * properties.ShaftElasticity);
        }

        private void OnDataLayerReady()
        {
            this.inverters = this.digitalDevicesDataProvider
             .GetAllInverters()
             .Select<Inverter, IInverterStatusBase>(i =>
             {
                 switch (i.Type)
                 {
                     case InverterType.Acu:
                         return new AcuInverterStatus(i.Index);

                     case InverterType.Ang:
                         return new AngInverterStatus(i.Index);

                     case InverterType.Agl:
                         return new AglInverterStatus(i.Index);

                     default:
                         throw new Exception();
                 }
             })
             .ToArray();

            if (this.inverters.SingleOrDefault(i => i.SystemIndex == InverterIndex.MainInverter) as IAngInverterStatus is null)
            {
                throw new Exception("No main inverter is configured in the system.");
            }
        }

        #endregion
    }
}
