using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Information)]
    internal sealed class ReleaseViewModel : BaseAboutMenuViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IBarcodeReaderService barcodeReaderService;

        private readonly IMachineErrorsService machineErrorsService;

        private readonly ISessionService sessionService;

        #endregion

        #region Constructors

        public ReleaseViewModel(
            ISessionService sessionService,
            IMachineErrorsService machineErrorsService,
            IAuthenticationService authenticationService,
            IBarcodeReaderService barcodeReaderService)
            : base()
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineErrorsService = machineErrorsService;
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.barcodeReaderService = barcodeReaderService ?? throw new ArgumentNullException(nameof(barcodeReaderService));
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.ReadNotes();
        }

        protected override async Task OnDataRefreshAsync()
        {
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private void ReadNotes()
        {
            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var notes = File.ReadAllText(path + "\\ReleaseNotes.html");
        }

        #endregion
    }
}
