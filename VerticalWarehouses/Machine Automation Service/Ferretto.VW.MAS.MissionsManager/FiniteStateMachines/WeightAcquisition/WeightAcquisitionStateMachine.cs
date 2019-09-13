using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.Utils;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.MissionsManager.FiniteStateMachines
{
    internal class WeightAcquisitionStateMachine : FiniteStateMachine<IWeightAcquisitionMoveToStartPositionState>,
        IWeightAcquisitionStateMachine
    {

        #region Fields

        private bool abortRequested;

        #endregion

        #region Constructors

        public WeightAcquisitionStateMachine(
            BayNumber requestingBay,
            IEventAggregator eventAggregator,
            ILogger<StateBase> logger,
            IServiceScopeFactory serviceScopeFactory)
            : base(requestingBay, eventAggregator, logger, serviceScopeFactory)
        {
        }

        #endregion



        #region Methods

        protected override bool FilterCommand(CommandMessage command)
        {
            return command.Type == MessageType.WeightAcquisitionCommand;
        }

        protected override bool FilterNotification(NotificationMessage notification)
        {
            return
                notification.Type == MessageType.CurrentSamplingInMotionNotification
                ||
                notification.Type == MessageType.CurrentSamplingInPlaceNotification
                ||
                notification.Type == MessageType.Positioning;
        }

        protected override IState OnCommandReceived(CommandMessage command)
        {
            var newState = base.OnCommandReceived(command);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            if (command.Data is WeightAcquisitionCommandMessageData commandData
                &&
                commandData.CommandAction == CommonUtils.CommandAction.Abort)
            {
                this.abortRequested = true; // TODO send abort command to FSMs
            }

            return newState;
        }

        protected override IState OnNotificationReceived(NotificationMessage notification)
        {
            var newState = base.OnNotificationReceived(notification);
            if (newState != this.ActiveState)
            {
                return newState;
            }

            switch (this.ActiveState)
            {
                case IWeightAcquisitionMoveToStartPositionState _:
                    {
                        if (this.abortRequested)
                        {
                            return this.GetState<IWeightAcquisitionMoveBackToStartPositionState>();
                        }

                        if (notification.Type == MessageType.Positioning
                            &&
                            notification.Status == MessageStatus.OperationEnd
                            &&
                            notification.Data is PositioningMessageData positioning
                            &&
                            positioning.AxisMovement == Axis.Vertical)
                        {
                            return this.GetState<IWeightAcquisitionInPlaceSamplingState>();
                        }

                        break;
                    }

                case IWeightAcquisitionInPlaceSamplingState _:
                    {
                        if (this.abortRequested)
                        {
                            return this.GetState<IWeightAcquisitionMoveBackToStartPositionState>();
                        }

                        if (notification.Type == MessageType.CurrentSamplingInPlaceNotification
                            &&
                            notification.Status == MessageStatus.OperationEnd)
                        {
                            return this.GetState<IWeightAcquisitionInMotionSamplingState>();
                        }

                        break;
                    }

                case IWeightAcquisitionInMotionSamplingState _:
                    {
                        if (this.abortRequested)
                        {
                            return this.GetState<IWeightAcquisitionMoveBackToStartPositionState>();
                        }

                        if (notification.Type == MessageType.CurrentSamplingInMotionNotification
                            &&
                            notification.Status == MessageStatus.OperationEnd)
                        {
                            return this.GetState<IWeightAcquisitionMoveBackToStartPositionState>();
                        }

                        break;
                    }

                case IWeightAcquisitionMoveBackToStartPositionState _:
                    {
                        if (notification.Type == MessageType.Positioning
                            &&
                            notification.Status == MessageStatus.OperationEnd
                            &&
                            notification.Data is PositioningMessageData positioning
                            &&
                            positioning.AxisMovement == Axis.Vertical)
                        {
                            this.RaiseCompleted();

                            return null;
                        }

                        break;
                    }
            }

            return newState;
        }

        #endregion
    }
}
