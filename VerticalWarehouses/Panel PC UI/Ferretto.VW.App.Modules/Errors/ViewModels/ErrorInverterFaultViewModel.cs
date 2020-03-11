using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Errors.ViewModels
{
    internal sealed class ErrorInverterFaultViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineErrorsWebService machineErrorsWebService;

        private readonly INavigationService navigationService;

        private MachineError error;

        private string errorTime;

        private ICommand markAsResolvedCommand;

        #endregion

        #region Constructors

        public ErrorInverterFaultViewModel(IMachineErrorsWebService machineErrorsWebService, INavigationService navigationService)
            : base(Services.PresentationMode.Menu | Services.PresentationMode.Installer | Services.PresentationMode.Operator)
        {
            this.machineErrorsWebService = machineErrorsWebService ?? throw new ArgumentNullException(nameof(machineErrorsWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            new Timer(this.OnErrorChanged, null, 0, 30 * 1000);
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public MachineError Error
        {
            get => this.error;
            set => this.SetProperty(ref this.error, value, () => this.OnErrorChanged(null));
        }

        public string ErrorTime
        {
            get => this.errorTime;
            set => this.SetProperty(ref this.errorTime, value);
        }

        public ICommand MarkAsResolvedCommand =>
            this.markAsResolvedCommand
            ??
            (this.markAsResolvedCommand = new DelegateCommand(
                async () => await this.MarkAsResolvedAsync(),
                this.CanMarkAsResolved)
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
            await base.OnAppearedAsync();

            await this.RetrieveErrorAsync();

            this.IsBackNavigationAllowed = true;
        }

        private bool CanMarkAsResolved()
        {
            return
                this.Error != null
                &&
                !this.IsWaitingForResponse;
        }

        private async Task MarkAsResolvedAsync()
        {
            if (this.Error is null)
            {
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineErrorsWebService.ResolveAsync(this.Error.Id);
                //await this.machineErrorsWebService.ResolveAllAsync();

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();

                if (this.Error == null)
                {
                    await Application.Current.Dispatcher.BeginInvoke(
                        System.Windows.Threading.DispatcherPriority.ApplicationIdle,
                        new Action(() =>
                        {
                            if (this.navigationService.IsActiveView(nameof(Utils.Modules.Errors), Utils.Modules.Errors.ERRORINVERTERFAULT))
                            {
                                this.navigationService.GoBack();
                            }
                        }));
                }
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
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
                this.ErrorTime = Resources.General.Now;
            }
            else if (elapsedTime.TotalHours < 1)
            {
                this.ErrorTime = string.Format(Resources.General.MinutesAgo, elapsedTime.TotalMinutes);
            }
            else if (elapsedTime.TotalDays < 1)
            {
                this.ErrorTime = string.Format(Resources.General.HoursAgo, elapsedTime.TotalHours);
            }
            else
            {
                this.ErrorTime = string.Format(Resources.General.DaysAgo, elapsedTime.TotalDays);
            }
        }

        private async Task RetrieveErrorAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;

                this.Error = await this.machineErrorsWebService.GetCurrentAsync();
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
