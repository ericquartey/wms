using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DeviceManager.Positioning.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.Utils.Enumerations;
using Ferretto.VW.MAS.Utils.Messages;
using Ferretto.VW.MAS.Utils.Messages.FieldData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.DeviceManager.Positioning
{
    internal class PositioningProfileState : StateBase
    {
        #region Fields

        private const int MAX_RETRIES = 3;

        private const double tolerance = 2.5;

        private readonly IBaysDataProvider baysDataProvider;

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly ILoadingUnitsDataProvider loadingUnitProvider;

        private readonly Machine machineConfiguration;

        private readonly IPositioningMachineData machineData;

        /// <summary>
        /// TODO make minHeight a configuration parameter?
        /// </summary>
        private readonly double minHeight = 25.0;

        private readonly IServiceScope scope;

        private readonly IPositioningStateData stateData;

        private InverterIndex inverterIndex;

        private int retry;

        #endregion

        #region Constructors

        public PositioningProfileState(IPositioningStateData stateData, ILogger logger)
            : base(stateData.ParentMachine, logger)
        {
            this.stateData = stateData;
            this.machineData = stateData.MachineData as IPositioningMachineData;
            this.scope = this.ParentStateMachine.ServiceScopeFactory.CreateScope();
            this.baysDataProvider = this.scope.ServiceProvider.GetRequiredService<IBaysDataProvider>();
            this.loadingUnitProvider = this.scope.ServiceProvider.GetRequiredService<ILoadingUnitsDataProvider>();
            this.elevatorDataProvider = this.scope.ServiceProvider.GetRequiredService<IElevatorDataProvider>();
            this.machineConfiguration = this.scope.ServiceProvider.GetRequiredService<IMachineProvider>().Get();
        }

        #endregion

        #region Methods

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
                            var profileHeight = this.baysDataProvider.ConvertProfileToHeightNew(data.Profile, this.machineData.MessageData.SourceBayPositionId.Value);
                            //profileHeight = 174;    // TEST
                            this.Logger.LogInformation($"Height measured {profileHeight}mm. Profile {data.Profile / 100.0}%");
                            if ((profileHeight < this.minHeight - tolerance)
                                || data.Profile > 10000
                                || (profileHeight < this.machineConfiguration.LoadUnitMinHeight - tolerance)
                                || (profileHeight > this.machineConfiguration.LoadUnitMaxHeight + tolerance)
                                )
                            {
                                this.Logger.LogError($"Measure Profile error {profileHeight}! min height {this.machineConfiguration.LoadUnitMinHeight}, max height {this.machineConfiguration.LoadUnitMaxHeight}");
                                if (++this.retry < MAX_RETRIES)
                                {
                                    this.RequestMeasureProfile();
                                    break;
                                }
                            }
                            var loadUnitId = this.machineData.MessageData.LoadingUnitId;
                            if (!loadUnitId.HasValue)
                            {
                                var bayPosition = this.elevatorDataProvider.GetCurrentBayPosition();
                                var loadingUnitOnElevator = this.elevatorDataProvider.GetLoadingUnitOnBoard();
                                if (bayPosition != null
                                    && bayPosition.LoadingUnit != null
                                    && loadingUnitOnElevator is null
                                    )
                                {
                                    // manual pickup from bay
                                    loadUnitId = bayPosition.LoadingUnit.Id;
                                }
                            }
                            if (loadUnitId.HasValue)
                            {
                                this.loadingUnitProvider.SetHeight(loadUnitId.Value, profileHeight);
                            }
                            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
                        }
                        else if (message.Source == FieldMessageActor.IoDriver)
                        {
                            // we send the first request to read the height only after IoDriver has reset the reading enable signal
                            this.RequestMeasureProfile();
                        }
                        break;

                    case MessageStatus.OperationError:
                        this.stateData.FieldMessage = message;
                        this.Logger.LogError($"Measure Profile OperationError!");
                        this.ParentStateMachine.ChangeState(new PositioningErrorState(this.stateData, this.Logger));
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
        }

        public override void Stop(StopRequestReason reason)
        {
            this.Logger.LogDebug("1:Stop Method Start");

            this.stateData.StopRequestReason = reason;
            this.ParentStateMachine.ChangeState(new PositioningEndState(this.stateData, this.Logger));
        }

        private void RequestMeasureProfile()
        {
            this.Logger.LogDebug($"Request MeasureProfile {this.retry + 1}");

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
