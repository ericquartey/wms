using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Installation.ViewsAndViewModels.SingleViews;
using Ferretto.VW.App.Services;
using Ferretto.VW.App.Services.Interfaces;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs;
using Ferretto.VW.MAS.AutomationService.Contracts.Hubs.EventArgs;
using Unity;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationDebug : BasePresentation
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
            if (container == null)
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
                var win = new DiagnosticDetailsView();
                win.DataContext = this.diagnosticDetailsViewModel;
                win.Loaded += async (s, e) => await this.diagnosticDetailsViewModel.OnEnterViewAsync();
                win.Unloaded += (s, e) => this.diagnosticDetailsViewModel.UnSubscribeMethodFromEvent();
                win.Show();
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}
