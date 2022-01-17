using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Enumerations;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver
{
    internal class InvertersProvider : IInvertersProvider
    {
        #region Fields

        private static IEnumerable<IInverterStatusBase> inverters;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger<InverterDriverService> logger;

        private readonly IMachineProvider machineProvider;

        private readonly IServiceScopeFactory serviceScopeFactory;

        #endregion

        #region Constructors

        public InvertersProvider(
            IEventAggregator eventAggregator,
            IElevatorDataProvider elevatorDataProvider,
            IMachineProvider machineProvider,
            IBaysDataProvider baysDataProvider,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IErrorsProvider errorsProvider,
            IDataLayerService dataLayerService,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<InverterDriverService> logger
            )
        {
            this.eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator)); ;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.machineProvider = machineProvider ?? throw new ArgumentNullException(nameof(machineProvider));
            this.baysDataProvider = baysDataProvider ?? throw new ArgumentNullException(nameof(baysDataProvider));
            this.digitalDevicesDataProvider = digitalDevicesDataProvider ?? throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
            this.errorsProvider = errorsProvider ?? throw new ArgumentNullException(nameof(errorsProvider));
            this.serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));

            if (dataLayerService.IsReady)
            {
                this.OnDataLayerReady();
            }
            else
            {
                this.eventAggregator.GetEvent<NotificationEvent>().Subscribe((x) =>
                    this.OnDataLayerReady(),
                    ThreadOption.PublisherThread,
                    false,
                    m => m.Type is MessageType.DataLayerReady);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Computes the elevator displacement due to belt and shaft mechanical distorsions.
        /// </summary>
        /// <param name="targetPosition">The vertical position of the elevator, in millimeters.</param>
        /// <returns>The vertical position displacement, in millimeters.</returns>
        public double ComputeDisplacement(double targetPosition, out double weight)
        {
            weight = 0;
            var loadingUnit = this.elevatorDataProvider.GetLoadingUnitOnBoard();
            if (loadingUnit is null)
            {
                return 0;
            }
            weight = loadingUnit.GrossWeight;
            var shaftTorsion = this.ComputeShaftTorsion(loadingUnit.GrossWeight);
            var beltElongation = this.ComputeBeltElongation(loadingUnit.GrossWeight, targetPosition);

            return beltElongation + shaftTorsion;
        }

        public double ComputeDisplacement(double targetPosition, double weight)
        {
            var shaftTorsion = this.ComputeShaftTorsion(weight);
            var beltElongation = this.ComputeBeltElongation(weight, targetPosition);

            return beltElongation + shaftTorsion;
        }

        public int ComputePositioningValues(
            IInverterStatusBase inverter,
            IPositioningFieldMessageData positioningData,
            Orientation axisOrientation,
            int currentPosition,
            bool refreshTargetTable,
            out InverterPositioningFieldMessageData positioningFieldData)
        {
            int targetPosition;
            var position = positioningData.TargetPositionOriginal;
            if (positioningData.MovementType == MovementType.Absolute)
            {
                if (positioningData.AxisMovement != Axis.BayChain)  // REMOVE
                {                                                   // REMOVE
                    var axis = positioningData.AxisMovement == Axis.Horizontal
                        ? this.elevatorDataProvider.GetAxis(Orientation.Horizontal)
                        : this.elevatorDataProvider.GetAxis(Orientation.Vertical);

                    if (position < axis.LowerBound && !positioningData.IsHorizontalCalibrate)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.DestinationBelowLowerBound, this.baysDataProvider.GetByInverterIndex(inverter.SystemIndex));
                        throw new InvalidOperationException($"The requested position ({position:0.00}) is less than the axis lower bound ({axis.LowerBound:0.00}).");
                    }
                    if (position > axis.UpperBound && !positioningData.IsHorizontalCalibrate)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.DestinationOverUpperBound, this.baysDataProvider.GetByInverterIndex(inverter.SystemIndex));
                        throw new InvalidOperationException($"The requested position ({position:0.00}) is greater than the axis upper bound ({axis.UpperBound}:0.00).");
                    }

                    position -= axis.Offset;
                    if (axis.Orientation == Orientation.Vertical)
                    {
                        if (positioningData.ComputeElongation)
                        {
                            var beltDisplacement = this.ComputeDisplacement(positioningData.TargetPositionOriginal, out var weight);
                            this.logger.LogInformation($"Vertical positioning with Belt elongation for height={positioningData.TargetPositionOriginal:0.00} and weight={weight:0.00} is {beltDisplacement:0.00} [mm]. VerticalDepositOffset is {axis.VerticalDepositOffset:0.00} [mm].");
                            position += beltDisplacement;
                            if (axis.VerticalDepositOffset.HasValue)
                            {
                                position += axis.VerticalDepositOffset.Value;
                            }
                        }
                        else if (positioningData.IsPickupMission
                            && axis.VerticalPickupOffset.HasValue
                            )
                        {
                            this.logger.LogInformation($"Vertical positioning for height={positioningData.TargetPosition:0.00}. VerticalPickupOffset is {axis.VerticalPickupOffset:0.00} [mm].");
                            position += axis.VerticalPickupOffset.Value;
                        }
                    }
                } // REMOVE

                if (positioningData.AxisMovement == Axis.BayChain)  // ADD THIS SNIPPET CODE
                {
                    // Check lower position (Absolute scale)
                    const double INTERNAL_LIMIT = -150.0d;
                    if (position < INTERNAL_LIMIT)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.DestinationBelowLowerBound, this.baysDataProvider.GetByInverterIndex(inverter.SystemIndex));
                        throw new InvalidOperationException($"The requested position ({position:0.00}) is less than the bay internal limit ({INTERNAL_LIMIT:0.00}).");
                    }
                    // Check upper position
                    const double EXTERNAL_MAX_LIMIT = 1350.0d;
                    if (position > EXTERNAL_MAX_LIMIT)
                    {
                        this.errorsProvider.RecordNew(MachineErrorCode.DestinationOverUpperBound, this.baysDataProvider.GetByInverterIndex(inverter.SystemIndex));
                        throw new InvalidOperationException($"The requested position ({position:0.00}) is greater than the bay external limit ({EXTERNAL_MAX_LIMIT}:0.00).");
                    }
                }
            }
            if (positioningData.AxisMovement == Axis.BayChain)
            {
                targetPosition = (int)Math.Round(this.baysDataProvider.GetResolution(inverter.SystemIndex) * position);
            }
            else
            {
                targetPosition = this.ConvertMillimetersToPulses(position, axisOrientation);
            }

            int[] targetAcceleration; int[] targetDeceleration;
            int[] targetSpeed; int[] switchPosition;

            if (positioningData.AxisMovement == Axis.BayChain)
            {
                targetAcceleration = positioningData.TargetAcceleration
                    .Select(value => this.ConvertMillimetersToPulses(value, inverter))
                    .ToArray();
                targetDeceleration = positioningData.TargetDeceleration
                    .Select(value => this.ConvertMillimetersToPulses(value, inverter))
                    .ToArray();
                targetSpeed = positioningData.TargetSpeed
                    .Select(value => this.ConvertMillimetersToPulses(value, inverter))
                    .ToArray();
                switchPosition = positioningData.SwitchPosition
                    .Select(value => this.ConvertMillimetersToPulses(value, inverter))
                    .ToArray();
            }
            else
            {
                targetAcceleration = positioningData.TargetAcceleration
                    .Select(value => this.ConvertMillimetersToPulses(value, axisOrientation))
                    .ToArray();
                targetDeceleration = positioningData.TargetDeceleration
                    .Select(value => this.ConvertMillimetersToPulses(value, axisOrientation))
                    .ToArray();
                targetSpeed = positioningData.TargetSpeed
                    .Select(value => this.ConvertMillimetersToPulses(value, axisOrientation))
                    .ToArray();
                switchPosition = positioningData.SwitchPosition
                    .Select(value => this.ConvertMillimetersToPulses(value, axisOrientation))
                    .ToArray();
            }

            var direction = (int)((positioningData.Direction == HorizontalMovementDirection.Forwards) ? InverterMovementDirection.Forwards : InverterMovementDirection.Backwards);

            this.logger.LogDebug($"Direction: {positioningData.Direction}");
            this.logger.LogDebug($"Position:\t    Speed\t    Acceleration\t    Deceleration");
            for (var i = 0; i < positioningData.SwitchPosition.Length; i++)
            {
                if (positioningData.TargetSpeed[i] == 0)
                {
                    throw new InvalidOperationException($"The Speed of position {i} can not be zero.");
                }
                if (positioningData.TargetAcceleration[i] == 0)
                {
                    throw new InvalidOperationException($"The Acceleration of position {i} can not be zero.");
                }
                if (positioningData.TargetDeceleration[i] == 0)
                {
                    throw new InvalidOperationException($"The Deceleration of position {i} can not be zero.");
                }
                this.logger.LogDebug($"{positioningData.SwitchPosition[i]:0.00} mm,\t {positioningData.TargetSpeed[i]:0.00} mm/s,\t {positioningData.TargetAcceleration[i]:0.00} mm/s2,\t {positioningData.TargetDeceleration[i]:0.00} mm/s2");
            }

            positioningFieldData = new InverterPositioningFieldMessageData(
                positioningData,
                targetAcceleration,
                targetDeceleration,
                currentPosition,
                targetPosition,
                positioningData.TargetPosition,
                targetSpeed,
                switchPosition,
                direction,
                refreshTargetTable);
            this.logger.LogTrace($"1:CurrentPositionAxis = {currentPosition}");
            this.logger.LogTrace($"2:data.TargetPosition = {positioningFieldData.TargetPosition}");
            return targetPosition;
        }

        public int ConvertMillimetersToPulses(double millimeters, Orientation orientation)
        {
            var axis = this.elevatorDataProvider.GetAxis(orientation);

            return (int)Math.Round(axis.Resolution * millimeters);
        }

        public int ConvertMillimetersToPulses(double millimeters, IInverterStatusBase inverter)
        {
            return (int)Math.Round(this.baysDataProvider.GetResolution(inverter.SystemIndex) * millimeters);
        }

        public int ConvertMillimetersToPulses(double millimeters, InverterIndex bayInverterIndex)
        {
            return (int)Math.Round(this.baysDataProvider.GetResolution(bayInverterIndex) * millimeters);
        }

        public double ConvertPulsesToMillimeters(int pulses, Orientation orientation)
        {
            if (pulses == 0)
            {
                if (orientation == Orientation.Vertical)
                {
                    throw new ArgumentOutOfRangeException(nameof(pulses), "Pulses must be different from zero.");
                }
                return 0;
            }

            var axis = this.elevatorDataProvider.GetAxis(orientation);

            if (axis.Resolution == 0)
            {
                throw new InvalidOperationException(
                    $"Configured {orientation} axis resolution is zero, therefore it is not possible to convert pulses to millimeters.");
            }

            return pulses / axis.Resolution;
        }

        public double ConvertPulsesToMillimeters(int pulses, IInverterStatusBase inverter)
        {
            if (pulses == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pulses), "Pulses must be different from zero.");
            }

            var resolution = this.baysDataProvider.GetResolution(inverter.SystemIndex);

            if (resolution == 0)
            {
                throw new InvalidOperationException(
                    $"Configured inverter {inverter.SystemIndex} encoder resolution is zero, therefore it is not possible to convert pulses to millimeters.");
            }

            return (pulses / resolution);
        }

        public IEnumerable<IInverterStatusBase> GetAll()
        {
            if (inverters is null)
            {
                throw new InvalidOperationException("The inverter configuration is not yet loaded because data layer is not ready.");
            }

            return inverters;
        }

        public IInverterStatusBase GetByIndex(InverterIndex index)
        {
            var inverter = inverters.SingleOrDefault(i => i.SystemIndex == index);

            if (inverter is null)
            {
                throw new EntityNotFoundException(index.ToString());
            }

            return inverter;
        }

        public IAngInverterStatus GetMainInverter()
        {
            System.Diagnostics.Debug.Assert(inverters.Any(i => i.SystemIndex == InverterIndex.MainInverter));

            return inverters.Single(i => i.SystemIndex == InverterIndex.MainInverter) as IAngInverterStatus;
        }

        public IInverterStatusBase GetShutterInverter(BayNumber bayNumber)
        {
            var index = this.baysDataProvider.GetByNumber(bayNumber).Shutter.Inverter.Index;
            var inverter = inverters.SingleOrDefault(i => i.SystemIndex == index);

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

            var pulleysDistanceMeters = (machineHeight - ElevatorStructuralProperties.PulleysMargin) / 1000.0;

            var properties = this.elevatorDataProvider.GetStructuralProperties();

            var beltSpacingMeters = properties.BeltSpacing / 1000.0;

            var targetPositionMeters = targetPosition / 1000.0;

            var rigidityKgMeters = properties.BeltRigidity / (9.81 * 1000.0);

            var secondTermMultiplier = properties.SecondTermMultiplier; // 1 Vertimag 2017, 10 Vertimag EF, 6 = Todesco 56hz

            return
                grossWeight /
                (2 * (
                    (rigidityKgMeters / ((2 * pulleysDistanceMeters) - beltSpacingMeters - targetPositionMeters + ElevatorStructuralProperties.PulleyCircleMeters))
                        + (rigidityKgMeters / (secondTermMultiplier * targetPositionMeters))
                    )
                );
        }

        /// <summary>
        /// Computes the vertical position displacement due to the shaft torsion.
        /// </summary>
        /// <param name="grossWeight">The gross weight loaded on the elevator, in kilograms.</param>
        /// <returns>The vertical position displacement, in millimeters.</returns>
        private double ComputeShaftTorsion(double grossWeight)
        {
            var properties = this.elevatorDataProvider.GetStructuralProperties();

            const double m = 10.0 / 3.0;

            return
                64.0 * (m + 1) * (grossWeight * Math.Pow(properties.PulleyDiameter, 2) * properties.HalfShaftLength)
                /
                (Math.PI * Math.Pow(properties.ShaftDiameter, 4) * m * properties.ShaftElasticity);
        }

        private void OnDataLayerReady()
        {
            if (inverters != null)
            {
                // already initialized
                return;
            }
            this.logger.LogTrace("OnDataLayerReady start");

            // retrieve inverters configuration
            inverters = inverters ?? this.digitalDevicesDataProvider
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
                         return new AglInverterStatus(i.Index, this.serviceScopeFactory);

                     default:
                         throw new Exception();
                 }
             })
             .ToArray();

            if (inverters.SingleOrDefault(i => i.SystemIndex == InverterIndex.MainInverter) as IAngInverterStatus is null)
            {
                throw new Exception("No main inverter is configured in the system.");
            }
            this.logger.LogTrace("OnDataLayerReady end");
        }

        #endregion
    }
}
