using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveWaitDepositInternalBayStep : MissionMoveBase
    {
        #region Fields

        private readonly IMachineResourcesProvider machineResourcesProvider;

        #endregion

        #region Constructors

        public MissionMoveWaitDepositInternalBayStep(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
            this.machineResourcesProvider = this.ServiceProvider.GetRequiredService<IMachineResourcesProvider>();
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
        }

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.WaitDepositInternalBay;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.Status = MissionStatus.Waiting;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }

            var isLoadingUnitInExternalUpPosition = this.machineResourcesProvider.IsDrawerInBayTop(bay.Number);
            var isLoadingUnitInExternalDownPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bay.Number);
            var isLoadingUnitInInternalUpPosition = this.machineResourcesProvider.IsDrawerInBayInternalTop(bay.Number);
            var isLoadingUnitInInternalDownPosition = this.machineResourcesProvider.IsDrawerInBayInternalBottom(bay.Number);

            bool isSourceUp = this.Mission.LoadUnitSource == LoadingUnitLocation.InternalBay1Up ||
                this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay2Up ||
                this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay3Up;

            if ((!isSourceUp && isLoadingUnitInExternalDownPosition && !isLoadingUnitInInternalUpPosition) ||
                (isSourceUp && isLoadingUnitInExternalUpPosition && !isLoadingUnitInInternalDownPosition))
            {
                var description = $"Deposit in internalbay not possible because there is a LU in elevator. Wait for resume";
                this.Logger.LogInformation(description);
                return true;
            }

            // no need to wait
            var newStep = new MissionMoveDoubleExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);

            return true;
        }

        public override void OnNotification(NotificationMessage notification)
        {
            switch (notification.Status)
            {
                case MessageStatus.OperationStop:
                case MessageStatus.OperationError:
                case MessageStatus.OperationRunningStop:
                case MessageStatus.OperationFaultStop:
                    if (notification.RequestingBay == this.Mission.TargetBay || notification.RequestingBay == BayNumber.None)
                    {
                        this.OnStop(StopRequestReason.Error);
                    }
                    break;
            }
            switch (notification.Type)
            {
                case MessageType.Stop:
                    if (notification.RequestingBay == this.Mission.TargetBay
                        || notification.TargetBay == this.Mission.TargetBay
                        )
                    {
                        this.OnStop(StopRequestReason.Error);
                    }
                    break;
            }
        }

        public override void OnResume(CommandMessage command)
        {
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");
            var bay = this.BaysDataProvider.GetByLoadingUnitLocation(this.Mission.LoadUnitSource);
            if (bay is null)
            {
                this.ErrorsProvider.RecordNew(MachineErrorCode.LoadUnitSourceBay, this.Mission.TargetBay);
                throw new StateMachineException(ErrorDescriptions.LoadUnitSourceBay, this.Mission.TargetBay, MessageActor.MachineManager);
            }

            var isLoadingUnitInExternalUpPosition = this.machineResourcesProvider.IsDrawerInBayTop(bay.Number);
            var isLoadingUnitInExternalDownPosition = this.machineResourcesProvider.IsDrawerInBayBottom(bay.Number);
            var isLoadingUnitInInternalUpPosition = this.machineResourcesProvider.IsDrawerInBayInternalTop(bay.Number);
            var isLoadingUnitInInternalDownPosition = this.machineResourcesProvider.IsDrawerInBayInternalBottom(bay.Number);

            bool isSourceUp = this.Mission.LoadUnitSource == LoadingUnitLocation.InternalBay1Up ||
                this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay2Up ||
                this.Mission.LoadUnitDestination == LoadingUnitLocation.InternalBay3Up;

            if ((!isSourceUp && isLoadingUnitInExternalDownPosition && !isLoadingUnitInInternalUpPosition) ||
                (isSourceUp && isLoadingUnitInExternalUpPosition && !isLoadingUnitInInternalDownPosition))
            {
                var description = $"Deposit in internalbay not possible because there is a LU in elevator. Wait for resume";
                this.Logger.LogInformation(description);
                return;
            }

            // no need to wait
            var newStep = new MissionMoveDoubleExtBayStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);
        }

        #endregion
    }
}
