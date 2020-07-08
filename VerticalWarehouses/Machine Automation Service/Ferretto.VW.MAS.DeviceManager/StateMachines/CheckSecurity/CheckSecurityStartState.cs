using System;
using System.Collections.Generic;
using System.Linq;

using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.CheckSecurity.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.CheckSecurity
{
    internal class CheckSecurityStartState : StateBase
    {
        #region Fields

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IErrorsProvider errorsProvider;

        private readonly Dictionary<InverterIndex, MessageStatus> inverterResponses = new Dictionary<InverterIndex, MessageStatus>();

        private readonly ICheckSecurityMachineData machineData;

        private readonly IServiceScope scope;

        private readonly ICheckSecurityStateData stateData;

        #endregion

        #region Constructors

        public CheckSecurityStartState(ICheckSecurityStateData stateData, ILogger logger)
            : base(stateData?.ParentMachine, logger)
        {
            this.stateData = stateData ?? throw new ArgumentNullException(nameof(stateData));
            this.machineData = stateData.MachineData as ICheckSecurityMachineData ?? throw new ArgumentNullException(nameof(stateData));
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.errorsProvider = this.scope.ServiceProvider.GetRequiredService<IErrorsProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            // do nothing
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            //if (this.inverterResponses.Values.Count == this.machineData.BayInverters.Count())
            //{
            //    if (this.inverterResponses.Values.Any(r => r != MessageStatus.OperationEnd))
            //    {
            //        this.stateData.FieldMessage = message;
            //        this.ParentStateMachine.ChangeState(new CheckSecurityErrorState(this.stateData, this.Logger));
            //    }
            //    else
            //    {
            //        this.ParentStateMachine.ChangeState(new CheckSecurityEndState(this.stateData, this.Logger));
            //    }
            //}
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            // do nothing
        }

        public override void Start()
        {
            this.Logger.LogDebug($"Start {this.GetType().Name}");

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
                $"Starting Check Security in bay {this.machineData.TargetBay}",
                MessageActor.Any,
                MessageActor.DeviceManager,
                MessageType.InverterFaultReset,
                this.machineData.RequestingBay,
                this.machineData.TargetBay,
                MessageStatus.OperationStart);

            this.ParentStateMachine.PublishNotificationMessage(notificationMessage);
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop");
            var ioCommandMessageData = new MeasureProfileFieldMessageData(false);
            var ioCommandMessage = new FieldCommandMessage(
                ioCommandMessageData,
                $"Check security Stop ",
                FieldMessageActor.IoDriver,
                FieldMessageActor.DeviceManager,
                FieldMessageType.MeasureProfile,
                (byte)this.baysDataProvider.GetIoDevice(this.machineData.RequestingBay));

            this.Logger.LogTrace($"1:Publishing Field Command Message {ioCommandMessage.Type} Destination {ioCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(ioCommandMessage);

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new CheckSecurityEndState(this.stateData, this.Logger));
        }

        #endregion
    }
}
