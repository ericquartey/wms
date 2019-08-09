using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public class BaseNavigationViewModel : ViewModelBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private IRegionNavigationJournal journal;

        #endregion

        #region Constructors

        public BaseNavigationViewModel()
        {
        }

        #endregion

        #region Properties

        public IEventAggregator EventAggregator => this.eventAggregator;

        public INavigationService NavigationService => this.navigationService;

        #endregion

        #region Methods

        public void GoBack()
        {
            this.journal?.GoBack();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            this.journal = navigationContext.NavigationService.Journal;
            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()?.Publish(new PresentationChangedMessage(this.journal));
        }

        public void SohwButton(bool isVisible)
        {
            var state = new Presentation()
            {
                Type = PresentationTypes.Back,
                IsVisible = isVisible
            };

            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>()?.Publish(new PresentationChangedMessage(state));
        }

        #endregion
    }
}
