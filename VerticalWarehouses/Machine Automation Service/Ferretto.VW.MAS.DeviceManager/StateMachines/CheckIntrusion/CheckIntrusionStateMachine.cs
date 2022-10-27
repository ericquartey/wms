using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.CheckIntrusion.Interfaces;
using Ferretto.VW.MAS.DeviceManager.CheckIntrusion.Models;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.DeviceManager.CheckIntrusion
{
    internal class CheckIntrusionStateMachine : StateMachineBase
    {
        #region Fields

        private readonly ICheckIntrusionMachineData machineData;

        #endregion

        #region Constructors

        public CheckIntrusionStateMachine(
            CommandMessage receivedMessage,
            IEventAggregator eventAggregator,
            ILogger logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(receivedMessage.TargetBay, eventAggregator, logger, serviceScopeFactory)
        {
            var scope = serviceScopeFactory.CreateScope();
            var baysDataProvider = scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            var machineResourcesProvider = scope.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();

            this.machineData = new CheckIntrusionMachineData(
                receivedMessage.RequestingBay,
                receivedMessage.TargetBay,
                machineResourcesProvider,
                baysDataProvider,
                eventAggregator,
                logger,
                serviceScopeFactory);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.CurrentState.ProcessCommandMessage(message);
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.CurrentState.ProcessFieldNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.CurrentState.ProcessNotificationMessage(message);
        }

        /// <inheritdoc/>
        public override void Start()
        {
            lock (this.CurrentState)
            {
                var stateData = new CheckIntrusionStateData(this, this.machineData);
                var bay = this.machineData.BaysDataProvider.GetByNumberShutter(this.machineData.TargetBay);
                if ((bay.Shutter != null
                        && bay.Shutter.Type != ShutterType.NotSpecified
                        )
                        || (this.machineData.TargetBay == BayNumber.BayOne && !this.machineData.MachineSensorStatus.IsDrawerInBay1Top)
                        || (this.machineData.TargetBay == BayNumber.BayTwo && !this.machineData.MachineSensorStatus.IsDrawerInBay2Top)
                        || (this.machineData.TargetBay == BayNumber.BayThree && !this.machineData.MachineSensorStatus.IsDrawerInBay3Top)
                    )
                {
                    // shutter is present
                    // or load unit is not in bay
                    this.Logger.LogDebug($"Bay {this.machineData.TargetBay} do not need to check light curtain for intrusion");
                    this.ChangeState(new CheckIntrusionEndState(stateData, this.Logger));
                }
                else
                {
                    // no shutter and load unit in bay: we can check security
                    this.ChangeState(new CheckIntrusionStartState(stateData, this.Logger));
                }
            }
        }

        public override void Stop(StopRequestReason reason)
        {
            lock (this.CurrentState)
            {
                this.CurrentState.Stop(reason);
            }
        }

        #endregion
    }
}
