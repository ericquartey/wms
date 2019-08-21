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

        private readonly IMachineErrorsService machineErrorsService;

        private Error error;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorDetailsViewModel(IMachineErrorsService machineErrorsService)
            : base(Services.PresentationMode.Installer)
        {
            if (machineErrorsService == null)
            {
                throw new ArgumentNullException(nameof(machineErrorsService));
            }

            this.machineErrorsService = machineErrorsService;
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

            this.IsBackNavigationAllowed = true;

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
                this.NavigationService.IsBusy = true;
                this.Error = await this.machineErrorsService.GetCurrentAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.NavigationService.IsBusy = false;
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
                this.NavigationService.IsBusy = true;

                await this.machineErrorsService.ResolveAsync(this.error.Id);

                var nextError = await this.machineErrorsService.GetCurrentAsync();

                this.NavigationService.IsBusy = false;

                if (nextError == null)
                {
                    this.NavigationService.GoBack();
                }

                this.Error = nextError;
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
                this.NavigationService.IsBusy = false;
            }
        }

        #endregion
    }
}
