using System;
using System.Threading;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Providers;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;

using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStartSamplingWhileMovingState : PositioningStartMovingState
    {
        #region Fields

        private const int SamplingInterval = 1;

        private readonly IInverterPositioningFieldMessageData data;

        private readonly ITorqueCurrentMeasurementsDataProvider measurementsProvider;

        private bool isStarted;

        private DateTime lastRequestTimeStamp;

        private TorqueCurrentMeasurementSession measurementSession;

        private Timer samplingTimer;

        private bool stopRequested;

        #endregion

        #region Constructors

        public PositioningStartSamplingWhileMovingState(
            IInverterPositioningFieldMessageData data,
            IInverterStateMachine parentStateMachine,
            IInverterStatusBase inverterStatus,
            ILogger logger)
            : base(parentStateMachine, inverterStatus, logger)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            this.data = data;

            this.measurementsProvider = this.ParentStateMachine
                .GetRequiredService<ITorqueCurrentMeasurementsDataProvider>();
        }

        #endregion

        #region Methods

        public override void Start()
        {
            if (this.isStarted)
            {
                throw new InvalidOperationException($"State {this.GetType().Name} is already started.");
            }

            this.isStarted = true;

            base.Start();

            this.measurementSession = this.measurementsProvider.AddMeasurementSession(
                this.data.LoadingUnitId,
                this.data.LoadedNetWeight);

            this.Logger.LogInformation("Starting sampling of torque current.");

            this.samplingTimer = new Timer(this.OnTimerTick, null, SamplingInterval, Timeout.Infinite);
        }

        public override void Stop()
        {
            if (this.stopRequested)
            {
                throw new InvalidOperationException($"State {this.GetType().Name} was already stopped.");
            }

            base.Stop();

            this.stopRequested = true;
            this.samplingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            base.ValidateCommandResponse(message);

            this.Logger.LogInformation($"Received response {message.ParameterId}, {message.SystemIndex}.");

            if (message.ParameterId == InverterParameterId.TorqueCurrent)
            {
                var sample = this.measurementsProvider.AddSample(
                    this.measurementSession.Id,
                    message.IntPayload,
                    DateTime.Now,
                    this.lastRequestTimeStamp);

                this.NotifyNewSample(sample);

                if (!this.stopRequested && !this.TargetPositionReached)
                {
                    this.samplingTimer.Change(SamplingInterval, Timeout.Infinite);
                }
            }

            return true;
        }

        protected override void OnDisposing()
        {
            base.OnDisposing();

            this.samplingTimer.Dispose();
            this.samplingTimer = null;
        }

        private void NotifyNewSample(TorqueCurrentSample sample)
        {
            this.ParentStateMachine.PublishNotificationEvent(
                new FieldNotificationMessage(
                    new PositioningFieldMessageData(
                        new PositioningMessageData
                        {
                            TorqueCurrentSample = new DataSample
                            {
                                TimeStamp = sample.TimeStamp,
                                Value = sample.Value
                            }
                        }),
                "New torque sample acquired",
                FieldMessageActor.Any,
                FieldMessageActor.InverterDriver,
                FieldMessageType.Positioning,
                MessageStatus.OperationExecuting,
                this.InverterStatus.SystemIndex,
                ErrorLevel.Info));
        }

        private void OnTimerTick(object state)
        {
            if (this.stopRequested)
            {
                return;
            }

            this.lastRequestTimeStamp = DateTime.Now;
            this.Logger.LogInformation("Requesting new sample.");

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    InverterParameterId.TorqueCurrent));
        }

        #endregion
    }
}
