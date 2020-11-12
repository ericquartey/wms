using System;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Resources;
using Ferretto.VW.MAS.Utils.Exceptions;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public class MissionMoveWaitDepositBayStep : MissionMoveBase
    {
        #region Constructors

        public MissionMoveWaitDepositBayStep(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
            : base(mission, serviceProvider, eventAggregator)
        {
        }

        #endregion

        #region Methods

        public override void OnCommand(CommandMessage command)
        {
            // TODO Add your implementation code here
        }

        public override bool OnEnter(CommandMessage command, bool showErrors = true)
        {
            // Transition of step: MissionMoveToTargetStep -> MissionMoveWaitDepositBayStep -> MissionMoveDepositUnitStep

            this.MachineProvider.UpdateMissionTime(DateTime.UtcNow - this.Mission.StepTime);

            this.Mission.EjectLoadUnit = false;
            this.Mission.RestoreStep = MissionStep.NotDefined;
            this.Mission.Step = MissionStep.WaitDepositBay;
            this.Mission.StepTime = DateTime.UtcNow;
            this.Mission.DeviceNotifications = MissionDeviceNotifications.None;
            this.Mission.StopReason = StopRequestReason.NoReason;
            this.Mission.CloseShutterBayNumber = BayNumber.None;
            this.MissionsDataProvider.Update(this.Mission);
            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            // get the (target) bay
            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);

            // upper location
            var upperBayLocation = bay.Positions.FirstOrDefault(p => p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
            if (upperBayLocation is LoadingUnitLocation.NoLocation)
            {
                // TODO Add warning log
            }
            // lower location
            var lowerBayLocation = bay.Positions.FirstOrDefault(p => !p.IsUpper)?.Location ?? LoadingUnitLocation.NoLocation;
            if (lowerBayLocation is LoadingUnitLocation.NoLocation)
            {
                // TODO Add warning log
            }
            var upperBayPosition = bay.Positions.FirstOrDefault(p => p.IsUpper);
            var isLoadingUnitOnUpperBayPosition = (upperBayPosition.LoadingUnit != null);

            // get the loading unit on the lower bay position
            var lowerBayPosition = bay.Positions.FirstOrDefault(p => !p.IsUpper);
            var isLoadingUnitOnLowerBayPosition = (lowerBayPosition.LoadingUnit != null);

            if ((isLoadingUnitOnUpperBayPosition || isLoadingUnitOnLowerBayPosition)
                && this.IsWaitingRequested(bay))
            {
                var reasonDescription = "";
                if (isLoadingUnitOnLowerBayPosition)
                {
                    reasonDescription = $"Reason: {lowerBayLocation} is occupied";
                }
                if (isLoadingUnitOnUpperBayPosition)
                {
                    reasonDescription = $"Reason: {upperBayLocation} is occupied";
                }
                var description = $"Deposit on Bay {bay.Number} not allowed at the moment in bay." + $" {reasonDescription}" + " Wait for resume";

                // we don't send the current LU in bay while operator is working on other position
                this.Logger.LogInformation(description);
                this.Mission.Status = MissionStatus.Waiting;

                this.MissionsDataProvider.Update(this.Mission);

                this.SendMoveNotification(
                    this.Mission.TargetBay,
                    this.Mission.Step.ToString(),
                    MessageStatus.OperationEnd
                );

                return true;
            }

            // No need to wait
            var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
            newStep.OnEnter(null);

            return true;
        }

        public override void OnNotification(NotificationMessage message)
        {
            // TODO Add your implementation code here
        }

        public override void OnResume(CommandMessage command)
        {
            // Transition of step: MissionMoveToTargetStep -> MissionMoveWaitDepositBayStep -> MissionMoveDepositUnitStep

            this.Logger.LogDebug($"{this.GetType().Name}: {this.Mission}");

            // get the (target) bay
            var bay = this.BaysDataProvider.GetByNumber(this.Mission.TargetBay);

            // are there waiting mission on the bay?
            if (!bay.Positions.Any(x => x.LoadingUnit != null)
                || !this.IsWaitingRequested(bay)
                )
            {
                this.Mission.Status = MissionStatus.Executing;

                // No need to wait, go ahead with no worries
                var newStep = new MissionMoveDepositUnitStep(this.Mission, this.ServiceProvider, this.EventAggregator);
                newStep.OnEnter(null);
            }
        }

        private bool IsWaitingRequested(Bay bay)
        {
            // List of waiting mission on the bay
            var waitMissions = this.MissionsDataProvider.GetAllActiveMissions()
                .Where(m =>
                    m.TargetBay == this.Mission.TargetBay &&
                    ((m.Status == MissionStatus.Waiting && m.Step == MissionStep.WaitPick)
                        || m.Status == MissionStatus.New
                        )
                );

            var isWaitingRequested = false;

            foreach (var m in waitMissions.Where(x => x.Status == MissionStatus.Waiting
                && x.Step == MissionStep.WaitPick
                && x.LoadUnitId != this.Mission.LoadUnitId)
                )
            {
                if (bay.Carousel == null
                    || this.SensorsProvider.IsLoadingUnitInLocation(this.Mission.LoadUnitDestination)
                    )

                {
                    // there are more missions when user has requestd to return to cell
                    if (waitMissions.Count(x => x.LoadUnitId == m.LoadUnitId) == 1)
                    {
                        this.Logger.LogInformation($"LU {m.LoadUnitId} is blocking");
                        isWaitingRequested = true;
                    }
                }
            }
            return isWaitingRequested;
        }

        #endregion
    }
}
