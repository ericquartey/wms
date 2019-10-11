using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationHelp : BasePresentationViewModel
    {
        #region Constructors

        public PresentationHelp()
            : base(PresentationTypes.Help)
        {
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            return Task.CompletedTask;
        }

        #endregion
    }
}
