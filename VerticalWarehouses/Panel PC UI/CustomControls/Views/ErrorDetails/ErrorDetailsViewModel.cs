using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Controls.Views.ErrorDetails
{
    public class ErrorDetailsViewModel : Controls.BaseViewModel
    {
        #region Fields

        private readonly IErrorsMachineService errorsMachineService;

        private Error error;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorDetailsViewModel(IErrorsMachineService errorsMachineService)
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

        private bool CanExecuteMarkAsResolvedCommand()
        {
            return this.error != null;
        }

        private async Task ExecuteMarkAsResolvedCommandAsync()
        {
            if (this.error == null)
            {
                return;
            }

            try
            {
                await this.errorsMachineService.ResolveAsync(this.error.Id);

                this.Error = await this.errorsMachineService.GetCurrentAsync();
            }
            catch
            {
                // TODO notify error on footer
            }
        }

        #endregion
    }
}
