using System.Threading.Tasks;
using CommonServiceLocator;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.App.Services;
using Unity;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationDebug : BasePresentationViewModel
    {
        #region Fields

        private readonly IUnityContainer container;

        private readonly DiagnosticDetailsViewModel diagnosticDetailsViewModel;

        #endregion

        #region Constructors

        public PresentationDebug(
            IUnityContainer container,
            DiagnosticDetailsViewModel model)
            : base(PresentationTypes.Debug)
        {
            if (container is null)
            {
                throw new System.ArgumentNullException(nameof(container));
            }

            this.container = container;
            this.diagnosticDetailsViewModel = model;
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            if (!this.diagnosticDetailsViewModel.IsOpen)
            {
                var dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
                dialogService.Show(
                    nameof(Utils.Modules.Installation),
                    Utils.Modules.Installation.Devices.DEVICES);
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
