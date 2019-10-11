using System;
using System.Threading;
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

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private Error error;

        private string errorTime;

        private bool isWaitingForResponse;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorDetailsViewModel(IMachineErrorsWebService machineErrorsWebService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));

            new Timer(this.OnErrorChanged, null, 0, 30 * 1000);
        }

        #endregion

        #region Properties

        public Error Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value, () => this.OnErrorChanged(null));
        }

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

        public ICommand MarkAsResolvedCommand =>
            this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.MarkAsResolvedAsync(),
                this.CanExecuteMarkAsResolvedCommand)
            .ObservesProperty(() => this.Error)
            .ObservesProperty(() => this.IsWaitingForResponse));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            this.Error = null;
        }

        public override async Task OnAppearedAsync()
        {
            await this.CheckErrorsPresenceAsync();

            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();
        }

        private bool CanExecuteMarkAsResolvedCommand()
        {
            return
                this.error != null
                &&
                !this.IsWaitingForResponse;
        }

        private async Task CheckErrorsPresenceAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private async Task MarkAsResolvedAsync()
        {
            if (this.error is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAsync(this.error.Id);

                var nextError = await this.machineErrorsWebService.GetCurrentAsync();
                if (nextError is null)
                {
                    this.NavigationService.GoBack();
                }
                else
                {
                    this.Error = nextError;
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        private void OnErrorChanged(object state)
        {
            if (this.error is null)
            {
                this.ErrorTime = null;
                return;
            }

            var elapsedTime = DateTime.UtcNow - this.error.OccurrenceDate;
            if (elapsedTime.TotalMinutes < 1)
            {
                this.ErrorTime = Resources.VWApp.Now;
            }
            else if (elapsedTime.TotalHours < 1)
            {
                this.ErrorTime = string.Format(Resources.VWApp.MinutesAgo, elapsedTime.TotalMinutes);
            }
            else if (elapsedTime.TotalDays < 1)
            {
                this.ErrorTime = string.Format(Resources.VWApp.HoursAgo, elapsedTime.TotalHours);
            }
            else
            {
                this.ErrorTime = string.Format(Resources.VWApp.DaysAgo, elapsedTime.TotalDays);
            }
        }

        #endregion
    }
}
