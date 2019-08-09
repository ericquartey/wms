using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Errors.ViewModels
{
    public class ErrorDetailsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IErrorsMachineService errorsMachineService;

        private Error error;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorDetailsViewModel(IErrorsMachineService errorsMachineService)
            : base(Services.PresentationMode.Installator)
        {
            if (errorsMachineService == null)
            {
                throw new ArgumentNullException(nameof(errorsMachineService));
            }

            this.errorsMachineService = errorsMachineService;
        }

        #endregion

        #region Properties

        public Error Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value);
        }

        public ICommand MarkAsResolvedCommand =>
            this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.ExecuteMarkAsResolvedCommandAsync(),
                this.CanExecuteMarkAsResolvedCommand)
            .ObservesProperty(() => this.Error));

        #endregion

        #region Methods

        public override async Task OnNavigatedAsync()
        {
            await this.CheckErrorsPresenceAsync();

            await base.OnNavigatedAsync();
        }

        private bool CanExecuteMarkAsResolvedCommand()
        {
            return this.error != null;
        }

        private async Task CheckErrorsPresenceAsync()
        {
            try
            {
                this.NavigationService.SetBusy(true);
                this.Error = await this.errorsMachineService.GetCurrentAsync();
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
            finally
            {
                this.NavigationService.SetBusy(false);
            }
        }

        private async Task ExecuteMarkAsResolvedCommandAsync()
        {
            if (this.error == null)
            {
                return;
            }

            try
            {
                this.NavigationService.SetBusy(true);
                await this.errorsMachineService.ResolveAsync(this.error.Id);

                var nextError = await this.errorsMachineService.GetCurrentAsync();
                this.NavigationService.SetBusy(false);
                if (nextError == null)
                {
                    this.NavigationService.GoBack();
                }

                this.Error = nextError;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
                this.NavigationService.SetBusy(false);
            }
        }

        #endregion
    }
}
