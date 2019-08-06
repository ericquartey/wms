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
        }

        #endregion

        #region Methods

        public override void Execute()
        {
            var journal = this.regionNavigationService.Journal;
            journal.GoBack();
        }

        #endregion
    }
}
