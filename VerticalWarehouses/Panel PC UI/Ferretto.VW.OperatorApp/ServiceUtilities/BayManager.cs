using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;

namespace Ferretto.VW.OperatorApp.ServiceUtilities
{
    public class BayManager : IBayManager
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public BayManager(
            IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.CurrentMission = null;
            this.QueuedMissionsQuantity = 0;

            this.eventAggregator.GetEvent<NotificationEventUI<ExecuteMissionMessageData>>().Subscribe(
                message => this.OnCurrentMissionChanged(message.Data.Mission, message.Data.MissionsQuantity));

            this.eventAggregator.GetEvent<NotificationEventUI<BayConnectedMessageData>>().Subscribe(
                message => this.OnBayConnected(message.Data));
        }

        #endregion

        #region Properties

        public int BayId { get; set; }

        public Mission CurrentMission { get; set; }

        public int QueuedMissionsQuantity { get; set; }

        #endregion

        #region Methods

        public void CompleteCurrentMission()
        {
            // TODO Implement mission completion logic
        }

        private void OnBayConnected(IBayConnectedMessageData data)
        {
            this.BayId = data.Id;
            this.QueuedMissionsQuantity = data.MissionQuantity;
        }

        private void OnCurrentMissionChanged(Mission mission, int missionsQuantity)
        {
            this.CurrentMission = mission;
            this.QueuedMissionsQuantity = missionsQuantity;
        }

        #endregion
    }
}
