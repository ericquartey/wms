using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;
using Prism.Events;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public abstract class MissionMoveBase : IMissionMoveBase
    {
        #region Constructors

        protected MissionMoveBase(Mission mission,
            IServiceProvider serviceProvider,
            IEventAggregator eventAggregator)
        {
            this.Mission = mission;
            this.ServiceProvider = serviceProvider;
            this.EventAggregator = eventAggregator;
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator { get; }

        public Mission Mission { get; set; }

        public IServiceProvider ServiceProvider { get; }

        #endregion

        #region Methods

        public abstract void OnCommand(CommandMessage command);

        public abstract bool OnEnter(CommandMessage command);

        public abstract void OnNotification(NotificationMessage message);

        public virtual void OnResume(CommandMessage command)
        {
        }

        public virtual void OnStop(StopRequestReason reason, bool moveBackward = false)
        {
            if (this.Mission != null)
            {
                this.Mission.StopReason = reason;

                if (this.GetType().Name != nameof(MissionMoveErrorState)
                    && this.Mission.IsRestoringType()
                    )
                {
                    this.Mission.FsmRestoreStateName = this.Mission.FsmStateName;
                    if (moveBackward)
                    {
                        this.Mission.NeedMovingBackward = true;
                    }
                    var newStep = new MissionMoveErrorState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
                else
                {
                    var newStep = new MissionMoveEndState(this.Mission, this.ServiceProvider, this.EventAggregator);
                    newStep.OnEnter(null);
                }
            }
        }

        public virtual bool UpdateResponseList(MessageType type)
        {
            bool update = false;
            switch (type)
            {
                case MessageType.Positioning:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Positioning;
                    update = true;
                    break;

                case MessageType.ShutterPositioning:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Shutter;
                    update = true;
                    break;

                case MessageType.Homing:
                    this.Mission.DeviceNotifications |= MissionDeviceNotifications.Homing;
                    update = true;
                    break;
            }
            return update;
        }

        #endregion
    }
}
