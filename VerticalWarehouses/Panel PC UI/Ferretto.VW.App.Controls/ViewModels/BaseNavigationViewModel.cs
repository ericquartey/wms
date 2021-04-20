using System;
using CommonServiceLocator;
using Ferretto.VW.App.Services;
using Prism.Events;
using Prism.Regions;

namespace Ferretto.VW.App.Controls
{
    public abstract class BaseNavigationViewModel : ViewModelBase
    {
        #region Fields

        private readonly IEventAggregator eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly INavigationService navigationService = ServiceLocator.Current.GetInstance<INavigationService>();

        private IRegionNavigationJournal journal;

        #endregion

        #region Constructors

        protected BaseNavigationViewModel()
        {
        }

        #endregion

        #region Events

        public event EventHandler<System.ComponentModel.CancelEventArgs> NavigatingBack;

        #endregion

        #region Properties

        public IEventAggregator EventAggregator => this.eventAggregator;

        public bool IsBackNavigationAllowed
        {
            set
            {
                var state = new Presentation
                {
                    Type = PresentationTypes.Back,
                    IsVisible = value
                };

                this.EventAggregator
                    .GetEvent<PresentationChangedPubSubEvent>()?
                    .Publish(new PresentationChangedMessage(state));
            }
        }

        public INavigationService NavigationService => this.navigationService;

        #endregion

        #region Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            this.journal = navigationContext.NavigationService.Journal;

            var parametersStorageKey = this.GetType().Name;
            if (navigationContext.Parameters.ContainsKey(parametersStorageKey))
            {
                this.Data = navigationContext.Parameters[parametersStorageKey];
            }
        }

        public override void OnNavigatingBack(BackNavigationContext navigationContext)
        {
            this.logger.Debug($"Navigating back");
            base.OnNavigatingBack(navigationContext);
            var args = new System.ComponentModel.CancelEventArgs(false);
            this.NavigatingBack?.Invoke(this, args);
            navigationContext.Cancel = args.Cancel;
        }

        #endregion
    }
}
