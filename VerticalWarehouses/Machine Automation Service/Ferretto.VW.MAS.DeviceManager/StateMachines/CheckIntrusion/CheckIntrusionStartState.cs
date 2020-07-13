using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CheckIntrusion.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CheckIntrusion
{
    internal class CheckIntrusionStartState : StateBase, IDisposable
    {
        #region Fields

        private const double tolerance = 2.5;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly ICheckIntrusionMachineData machineData;

        private readonly double minHeight = 25.0;

        private readonly IServiceScope scope;

        private readonly ICheckIntrusionStateData stateData;

        private int bayPositionId;

        private InverterIndex inverterIndex;

        private bool isDisposed;

        private Timer profileTimer;

        #endregion

        #region Constructors

        public CheckIntrusionStartState(ICheckIntrusionStateData stateData, ILogger logger)
            : base(stateData?.ParentMachine, logger)
        {
            this.stateData = stateData ?? throw new ArgumentNullException(nameof(stateData));
            this.machineData = stateData.MachineData as ICheckIntrusionMachineData ?? throw new ArgumentNullException(nameof(stateData));
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
        }

        #endregion

        #region Methods

        public void Dispose()
        {
            this.Dispose(true);
        }

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.MeasureProfile)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (message.Data is MeasureProfileFieldMessageData data && message.Source == FieldMessageActor.InverterDriver)
                        {
                            var profileHeight = this.baysDataProvider.ConvertProfileToHeightNew(data.Profile, this.bayPositionId);
                            this.Logger.LogDebug($"Height measured {profileHeight}mm. Profile {data.Profile / 100.0}%");
                            if ((profileHeight >= this.minHeight - tolerance)
                                && data.Profile <= 10000
                                )
                            {
                                this.Logger.LogError($"Intrusion detected in Bay {this.machineData.RequestingBay} by height {profileHeight}!");
                                this.errorsProvider.RecordNew(DataModels.MachineErrorCode.SecurityBarrierWasTriggered, this.machineData.RequestingBay);
                                // stop timers
                                this.profileTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                                this.ParentStateMachine.ChangeState(new CheckIntrusionErrorState(this.stateData, this.Logger));
                            }
                        }
                        else if (message.Source == FieldMessageActor.IoDriver)
                        {
                            // we enable the timer to request the height only after IoDriver has set the reading enable signal
                            this.profileTimer = new Timer(this.RequestMeasureProfile, null, 600, 600);
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.Logger.LogError($"Measure Profile OperationError!");
                        // stop timers
                        this.profileTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                        this.ParentStateMachine.ChangeState(new CheckIntrusionErrorState(this.stateData, this.Logger));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.inverterIndex = this.baysDataProvider.GetInverterIndexByProfile(this.machineData.RequestingBay);
            this.Logger.LogDebug($"Start {this.GetType().Name} Inverter {this.inverterIndex}");

            var bay = this.machineData.BaysDataProvider.GetByNumber(this.machineData.TargetBay);
            this.bayPositionId = bay.Positions.First(p => p.IsUpper).Id;

            var ioCommandMessageData = new MeasureProfileFieldMessageData(true);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Measure Profile Start ",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            var notificationMessage = new NotificationMessage(
                null,
                $"Starting Check Intrusion in bay {this.machineData.TargetBay}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.CheckIntrusion,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug($"1:Stop Method Start. Reason {reason}");

            // stop timers
            this.profileTimer?.Change(Timeout.Infinite, Timeout.Infinite);

            var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Check Intrusion Stop ",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new CheckIntrusionEndState(this.stateData, this.Logger));
        }

        protected void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                this.profileTimer?.Dispose();
                this.scope.Dispose();
            }

            this.isDisposed = true;
        }

        private void RequestMeasureProfile(object state)
        {
            this.Logger.LogDebug($"Request MeasureProfile");

            var inverterCommandMessageData = new MeasureProfileFieldMessageData();
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Measure Profile",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                (byte)this.inverterIndex);

            this.Logger.LogTrace($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);
        }

        #endregion
    }
}
