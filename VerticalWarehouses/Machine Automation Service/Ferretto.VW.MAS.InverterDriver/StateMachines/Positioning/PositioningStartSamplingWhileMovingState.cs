using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Ferretto.VW.MAS.Utils.Messages.FieldInterfaces;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.InverterDriver.StateMachines.Positioning
{
    internal class PositioningStartSamplingWhileMovingState : PositioningStartMovingState
    {
        #region Fields

        private readonly IInverterPositioningFieldMessageData data;

        private readonly ITorqueCurrentMeasurementsDataProvider measurementsProvider;

        private bool isStarted;

        private DateTime lastRequestTimeStamp;

        private TorqueCurrentMeasurementSession measurementSession;

        #endregion

        #region Constructors

        public PositioningStartSamplingWhileMovingState(
            IInverterPositioningFieldMessageData data,
            IInverterStateMachine parentStateMachine,
            IPositioningInverterStatus inverterStatus,
            ILogger logger)
            : base(parentStateMachine, data, inverterStatus, logger)
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

            this.RequestSample();
        }

        public override void Stop()
        {
            this.Logger.LogDebug("1:Positioning Stop requested");

            this.ParentStateMachine.ChangeState(
                new PositioningDisableOperationState(
                    this.ParentStateMachine,
                    this.InverterStatus as IPositioningInverterStatus,
                    this.Logger,
                    true));
        }

        public override bool ValidateCommandResponse(InverterMessage message)
        {
            base.ValidateCommandResponse(message);

            if (message.ParameterId == InverterParameterId.RMSCurrent)
            {
                var sample = this.measurementsProvider.AddSample(
                    this.measurementSession.Id,
                    message.UShortPayload / 10.0,
                    DateTime.Now,
                    this.lastRequestTimeStamp);

                this.NotifyNewSample(sample);

                if (!this.TargetPositionReached)
                {
                    this.RequestSample();
                }
            }

            return true;
        }

        private void NotifyNewSample(TorqueCurrentSample sample)
        {
            this.ParentStateMachine.PublishNotificationEvent(
                 new FieldNotificationMessage(
                     new InverterStatusUpdateFieldMessageData(
                         new DataSample
                         {
                             Value = sample.Value,
                             TimeStamp = sample.TimeStamp
                         }),
                 "Inverter Inputs update",
                 FieldMessageActor.DeviceManager,
                 FieldMessageActor.InverterDriver,
                 FieldMessageType.InverterStatusUpdate,
                 MessageStatus.OperationExecuting,
                 this.InverterStatus.SystemIndex));
        }

        private void RequestSample()
        {
            this.lastRequestTimeStamp = DateTime.Now;

            this.ParentStateMachine.EnqueueCommandMessage(
                new InverterMessage(
                    this.InverterStatus.SystemIndex,
                    InverterParameterId.RMSCurrent));
        }

        #endregion
    }
}
