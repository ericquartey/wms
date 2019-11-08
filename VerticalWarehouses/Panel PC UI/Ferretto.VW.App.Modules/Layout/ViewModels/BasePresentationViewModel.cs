using System.Collections.Generic;
using System.Linq;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Layout.ViewModels
{
    internal class BasePresentationViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly PresentationChangedPubSubEvent notificationEvent;

        private readonly List<IPresentation> states;

        private PresentationMode currentPresentation;

        private SubscriptionToken presentationEventSubscription;

        #endregion

        #region Constructors

        protected BasePresentationViewModel()
        {
            this.currentPresentation = PresentationMode.None;
            this.states = new List<IPresentation>();
            this.notificationEvent = this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>();

            this.presentationEventSubscription = this.notificationEvent.Subscribe(
                this.OnPresentationChanged,
                ThreadOption.UIThread,
                false);

            this.InitializeData();
        }

        #endregion

        #region Properties

        public PresentationMode CurrentPresentation
        {
            get => this.currentPresentation;
            set => this.SetProperty(ref this.currentPresentation, value);
        }

        public List<IPresentation> States => this.states;

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.presentationEventSubscription != null)
            {
                this.notificationEvent.Unsubscribe(this.presentationEventSubscription);
                this.presentationEventSubscription = null;
            }
        }

        public TPresentation GetInstance<TPresentation>() where TPresentation : IPresentation
        {
            return ServiceLocator.Current.GetInstance<TPresentation>();
        }

        public virtual void InitializeData()
        {
            // do nothing
        }

        public void OnPresentationChanged(PresentationChangedMessage presentation)
        {
            if (presentation is null)
            {
                throw new System.ArgumentNullException(nameof(presentation));
            }

            this.UpdatePresentation(presentation.Mode);

            this.UpdateChanges(presentation);
        }

        public void Show(PresentationTypes type, bool isVisible)
        {
            if (type == PresentationTypes.None)
            {
                this.states.ForEach(s => s.IsVisible = isVisible);
                return;
            }

            if (this.states.FirstOrDefault(s => s.Type == type) is IPresentation state)
            {
                state.IsVisible = isVisible;
            }
        }

        public virtual void UpdateChanges(PresentationChangedMessage presentation)
        {
            // do nothing
        }

        public virtual void UpdatePresentation(PresentationMode mode)
        {
            // do nothing
        }

        #endregion
    }
}
