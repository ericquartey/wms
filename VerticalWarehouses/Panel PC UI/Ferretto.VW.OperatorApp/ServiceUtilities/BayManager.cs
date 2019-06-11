using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.OperatorApp.Interfaces;
using Ferretto.VW.OperatorApp.ServiceUtilities.Interfaces;
using Ferretto.VW.OperatorApp.ViewsAndViewModels.DrawerOperations;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Events;
using Unity;

namespace Ferretto.VW.OperatorApp.ServiceUtilities
{
    public class BayManager : IBayManager
    {
        #region Fields

        private IUnityContainer container;

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public BayManager(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.CurrentMission = null;
            this.QueuedMissionsQuantity = 0;
        }

        #endregion

        #region Properties

        public Mission CurrentMission { get; set; }

        public int QueuedMissionsQuantity { get; set; }

        #endregion

        #region Methods

        public void CompleteCurrentMission()
        {
            // TODO Implement mission completion logic
        }

        public void Initialize(IUnityContainer container)
        {
            this.container = container;
            this.eventAggregator.GetEvent<NotificationEventUI<ExecuteMissionMessageData>>().Subscribe(
                message =>
                {
                    this.OnCurrentMissionChanged(message.Data.Mission, message.Data.MissionsQuantity);
                });
        }

        private void OnCurrentMissionChanged(Mission mission, int missionsQuantity)
        {
            this.CurrentMission = mission;
            this.QueuedMissionsQuantity = missionsQuantity;
            if (this.container.Resolve<IMainWindowViewModel>().ContentRegionCurrentViewModel is IDrawerActivityViewModel content)
            {
                content.UpdateView();
            }
        }

        #endregion
    }
}
