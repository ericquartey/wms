using System;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;

namespace Ferretto.VW.App.Modules.Layout.Presentation
{
    public class PresentationError : BasePresentationViewModel
    {
        #region Fields

        private readonly IMachineErrorsService machineErrorsService;

        private readonly INavigationService navigationService;

        #endregion

        #region Constructors

        public PresentationError(
            INavigationService navigationService,
            IMachineErrorsService machineErrorsService)
            : base(PresentationTypes.Error)
        {
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.machineErrorsService = machineErrorsService ?? throw new ArgumentNullException(nameof(machineErrorsService));
        }

        #endregion

        #region Methods

        public override Task ExecuteAsync()
        {
            this.navigationService.Appear(
            nameof(Utils.Modules.Errors),
            this.machineErrorsService.ViewErrorActive,
            data: null,
            trackCurrentView: true);

            return Task.CompletedTask;
        }

        #endregion
    }
}
