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

        private readonly IRegionNavigationService regionNavigationService;

        #endregion

        #region Constructors

        public PresentationBack(IRegionNavigationService regionNavigationService)
        {
            this.Type = PresentationTypes.Back;
            this.regionNavigationService = regionNavigationService;
            this.EventAggregator.GetEvent<PresentationChangedPubSubEvent>().Subscribe(this.Update);
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            var journal = this.regionNavigationService.Journal;
            journal.GoBack();
        }

        private void Update(PresentationChangedMessage message)
        {
            if (message.States != null &&
                message.States.FirstOrDefault(s => s.Type == this.Type) is Services.Presentation back)                
            {
                if (back.IsVisible.HasValue)
                {
                    this.IsVisible = back.IsVisible;
                }
            }
        }

        #endregion
    }
}
