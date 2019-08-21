using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationMachineMode : BasePresentation
    {
        #region Constructors

        public PresentationMachineMode()
            : base(PresentationTypes.MachineMode)
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
