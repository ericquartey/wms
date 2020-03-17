using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.HorizontalFwdBckw.Interfaces;
using Ferretto.VW.MAS.DeviceManager.HorizontalFwdBckw.Models;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.HorizontalFwdBckw
{
    internal class HorizontalFwdBckwStateMachine : StateMachineBase
    {
        #region Fields

        private readonly IHorizontalFwdBckwMachineData machineData;

        #endregion

        #region Constructors

        public HorizontalFwdBckwStateMachine(
            MessageActor requester,
            BayNumber requestingBay,
            BayNumber targetBay,
            IPositioningMessageData messageData,
            IMachineResourcesProvider machineResourcesProvider,
            IEventAggregator eventAggregator,
            ILogger logger,
            IBaysDataProvider baysDataProvider,
            IServiceScopeFactory serviceScopeFactory)
            : base(targetBay, eventAggregator, logger, serviceScopeFactory)
        {
            this.Logger.LogTrace("1:Method Start");

            this.Logger.LogTrace($"BayPosition = {targetBay}, position={messageData.TargetPosition} - MovementType = {messageData.MovementType}");

            this.machineData = new HorizontalFwdBckwMachineData(
                requester,
                requestingBay,
                targetBay,
                messageData,
                machineResourcesProvider,
                baysDataProvider.GetInverterIndexByMovementType(messageData, targetBay),
                eventAggregator,
                logger,
                baysDataProvider,
                serviceScopeFactory);
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public override void Start()
        {
            throw new NotImplementedException();
        }

        public override void Stop(StopRequestReason reason)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
