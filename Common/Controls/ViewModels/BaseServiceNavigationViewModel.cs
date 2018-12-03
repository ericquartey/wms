using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class BaseServiceNavigationViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly IHistoryViewService historyViewService = ServiceLocator.Current.GetInstance<IHistoryViewService>();
        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        #endregion Fields

        #region Properties

        public IEventService EventService => this.eventService;

        public IHistoryViewService HistoryViewService => this.historyViewService;

        public INavigationService NavigationService => this.navigationService;

        #endregion Properties

        #region Methods

        public override void Disappear()
        {
            this.navigationService.Disappear(this);

            base.Disappear();
        }

        #endregion Methods
    }
}
