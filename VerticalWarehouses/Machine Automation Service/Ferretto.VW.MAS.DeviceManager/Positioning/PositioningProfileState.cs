using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningProfileState : StateBase
    {
        #region Fields

        private readonly IBaysProvider baysProvider;

        /// <summary>
        /// profile = 200   ==> height = 0
        /// profile = 10000 ==> height = 75mm
        /// height = kMul * profile + kSum;
        /// </summary>
        private readonly double kMul = 0.007653;

        private readonly double kSum = -1.530612;

        private readonly ILoadingUnitsProvider loadingUnitProvider;

        private readonly IPositioningMachineData machineData;

        /// <summary>
        /// TODO make minHeight a configuration parameter?
        /// </summary>
        private readonly double minHeight = 5.0;

        private readonly IServiceScope scope;

        private readonly IPositioningStateData stateData;

        #endregion

        #region Constructors

        public PositioningProfileState(IPositioningStateData stateData)
            : base(stateData.ParentMachine, stateData.MachineData.Logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.baysProvider = this.scope.ServiceProvider.GetRequiredService<IBaysProvider>();
            this.loadingUnitProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsProvider>();
        }

        #endregion

        #region Methods

        public override void ProcessCommandMessage(CommandMessage message)
        {
            this.Logger.LogTrace($"1:Process Command Message {message.Type} Source {message.Source}");
        }

        public override void ProcessFieldNotificationMessage(FieldNotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Field Notification Message {message.Type} Source {message.Source} Status {message.Status}");

            if (message.Type == FieldMessageType.MeasureProfile)
            {
                switch (message.Status)
                {
                    case MessageStatus.OperationEnd:
                        if (message.Data is MeasureProfileFieldMessageData data)
                        {
                            var profileHeight = data.Profile * this.kMul + this.kSum;
                            this.Logger.LogInformation($"Height measured {profileHeight}mm. Profile {data.Profile / 100.0}%");
                            if (profileHeight < this.minHeight)
                            {
                                this.Logger.LogError($"Measure Profile error!");
                                this.Stop(StopRequestReason.Stop);
                                break;
                            }
                            if (this.machineData.MessageData.LoadingUnitId.HasValue)
                            {
                                this.loadingUnitProvider.SetHeight(this.machineData.MessageData.LoadingUnitId.Value, profileHeight);
                            }
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.Logger.LogError($"Measure Profile OperationError!");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData));
                        break;
                }
            }
        }

        public override void ProcessNotificationMessage(NotificationMessage message)
        {
            this.Logger.LogTrace($"1:Process Notification Message {message.Type} Source {message.Source} Status {message.Status}");
        }

        public override void Start()
        {
            var inverterIndex = this.baysProvider.GetInverterIndexByProfile(this.machineData.RequestingBay);

            var inverterCommandMessageData = new MeasureProfileFieldMessageData();
            var inverterCommandMessage = new FieldCommandMessage(
                inverterCommandMessageData,
                $"Measure Profile",
                FieldMessageActor.InverterDriver,
                FieldMessageActor.FiniteStateMachines,
                FieldMessageType.MeasureProfile,
                (byte)inverterIndex);

            this.Logger.LogTrace($"5:Publishing Field Command Message {inverterCommandMessage.Type} Destination {inverterCommandMessage.Destination}");

            this.ParentStateMachine.PublishFieldCommandMessage(inverterCommandMessage);

            this.Logger.LogDebug("Request MeasureProfile");
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData));
        }

        #endregion
    }
}
