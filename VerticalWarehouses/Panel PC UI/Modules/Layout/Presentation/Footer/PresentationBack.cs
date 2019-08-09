using System;
using System.Linq;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Prism.Regions;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationBack : BasePresentation
    {
        #region Fields

        private IRegionNavigationJournal journal;

        #endregion

        #region Constructors

        public PresentationBack()
        {
            if (regionNavigationService == null)
            {
                throw new ArgumentNullException(nameof(regionNavigationService));
            }

            this.regionNavigationService = regionNavigationService;

            this.Type = PresentationTypes.Back;

            this.EventAggregator
                .GetEvent<PresentationChangedPubSubEvent>()
                .Subscribe(this.Update);
            this.Type = PresentationTypes.Back;
            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>().Subscribe(this.Update);
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            this.journal?.GoBack();
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.Journal != null)
            {
                this.journal = message.Journal;
            }

            if (message.States != null &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation back)
            {
                if (back.IsVisible.HasValue)
            if (message.States != null
                &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation back
                &&
                back.IsVisible.HasValue)
            {
                this.IsVisible = back.IsVisible;
            }
        }

        #endregion
    }
}
