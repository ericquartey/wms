﻿using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.CalibrateAxis
{
    internal sealed class CalibrateAxisStateMachine : InverterStateMachineBase
    {
        #region Fields

        private readonly Axis axisToCalibrate;

        private readonly Calibration calibration;

        private readonly IInverterStatusBase inverterStatus;

        private Axis currentAxis;

        #endregion

        #region Constructors

        public CalibrateAxisStateMachine(
            Axis axisToCalibrate,
            Calibration calibration,
            IInverterStatusBase inverterStatus,
            ILogger logger,
            IEventAggregator eventAggregator,
            BlockingConcurrentQueue<InverterMessage> inverterCommandQueue,
            IServiceScopeFactory serviceScopeFactory)
            : base(logger, eventAggregator, inverterCommandQueue, serviceScopeFactory)
        {
            this.axisToCalibrate = axisToCalibrate;
            this.calibration = calibration;
            this.inverterStatus = inverterStatus;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        public override void Start()
        {
            this.Logger.LogTrace($"1:Axis to calibrate={this.axisToCalibrate}");

            switch (this.axisToCalibrate)
            {
                case Axis.HorizontalAndVertical:
                case Axis.Horizontal:
                    this.currentAxis = Axis.Horizontal;
                    break;

                case Axis.Vertical:
                    this.currentAxis = Axis.Vertical;
                    break;

                default:
                    this.currentAxis = this.axisToCalibrate;
                    break;
            }

            this.CurrentState = new CalibrateAxisStartState(this, this.currentAxis, this.calibration, this.inverterStatus, this.Logger);
            this.CurrentState?.Start();
        }

        /// <inheritdoc />
        public override void Stop()
        {
            this.CurrentState?.Stop();
        }

        #endregion
    }
}
