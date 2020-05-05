using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParameterInverterViewModel : BaseParameterInverterViewModel
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private IEnumerable<Inverter> configuration;

        private Inverter selectedInverter;

        private DelegateCommand setInvertersParamertersCommand;

        private DelegateCommand<object> showInverterParamertersCommand;

        #endregion

        #region Constructors

        public ParameterInverterViewModel(IMachineDevicesWebService machineDevicesWebService)
            : base()
        {
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public List<Inverter> Configuration => this.configuration?.ToList();

        public Inverter SelectedInverter
        {
            get => this.selectedInverter;
            set => this.SetProperty(ref this.selectedInverter, value);
        }

        public ICommand SetInvertersParamertersCommand =>
                   this.setInvertersParamertersCommand
               ??
               (this.setInvertersParamertersCommand = new DelegateCommand(
                async () => await this.SaveAllParametersAsync(), this.CanSave));

        public ICommand ShowInverterParamertersCommand =>
           this.showInverterParamertersCommand
       ??
       (this.showInverterParamertersCommand = new DelegateCommand<object>(this.ShowInverterParameters));

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            this.IsWaitingForResponse = false;
        }

        protected override async Task OnDataRefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                this.configuration = await this.machineDevicesWebService.GetParametersAsync();
                this.RaisePropertyChanged(nameof(this.Configuration));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.setInvertersParamertersCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private async Task SaveAllParametersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                await this.machineDevicesWebService.ProgramAllInvertersAsync(new VertimagConfiguration());

                this.ShowNotification(InstallationApp.InvertersProgrammingStarted, Services.Models.NotificationSeverity.Info);

                this.RaisePropertyChanged(nameof(this.Configuration));
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        private void ShowInverterParameters(object paramerter)
        {
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.PARAMETERSINVERTERDETAILS,
                data: paramerter,
                trackCurrentView: true);
        }

        #endregion
    }
}
