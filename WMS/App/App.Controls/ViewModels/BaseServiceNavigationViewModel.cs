using CommonServiceLocator;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.WMS.App.Controls.Interfaces;

namespace Ferretto.WMS.App.Controls
{
    public class BaseServiceNavigationViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();

        private readonly IHistoryViewService historyViewService = ServiceLocator.Current.GetInstance<IHistoryViewService>();

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        #endregion

        #region Properties

        public IEventService EventService => this.eventService;

        public IHistoryViewService HistoryViewService => this.historyViewService;

        public INavigationService NavigationService => this.navigationService;

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.navigationService.Disappear(this);

            base.Disappear();
        }

        #endregion
    }
}
