using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public class BaseServiceViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        #endregion

        #region Constructors

        public BaseServiceViewModel()
        {
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator => this.eventAggregator;

        public INavigationService NavigationService => this.navigationService;

        #endregion

        #region Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            var journal = navigationContext.NavigationService.Journal;
            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()?.Publish(new PresentationChangedMessage(journal));
        }

        #endregion
    }
}
