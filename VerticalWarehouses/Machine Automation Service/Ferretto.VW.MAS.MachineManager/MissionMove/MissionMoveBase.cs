using System;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.MachineManager.MissionMove.Interfaces;
using Ferretto.VW.MAS.Utils.Messages;

namespace Ferretto.VW.MAS.MachineManager.MissionMove
{
    public abstract class MissionMoveBase : IMissionMoveBase
    {
        #region Constructors

        protected MissionMoveBase(Mission mission,
            IServiceProvider serviceProvider)
        {
            this.Mission = mission;
            this.ServiceProvider = serviceProvider;
        }

        #endregion

        #region Properties

        public Mission Mission { get; set; }

        public IServiceProvider ServiceProvider { get; }

        #endregion

        #region Methods

        public abstract void OnCommand(CommandMessage command);

        public abstract bool OnEnter(CommandMessage command);

        public abstract void OnNotification(NotificationMessage message);

        #endregion
    }
}
