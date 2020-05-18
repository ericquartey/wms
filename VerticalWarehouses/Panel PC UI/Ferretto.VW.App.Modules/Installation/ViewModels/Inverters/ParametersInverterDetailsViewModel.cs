using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParametersInverterDetailsViewModel : BaseParameterInverterViewModel
    {
        #region Fields

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private InverterParametersData inverterParameters;

        private ISetVertimagConfiguration parentConfiguration;

        private DelegateCommand setInverterParamertersCommand;

        #endregion

        #region Constructors

        public ParametersInverterDetailsViewModel(IMachineDevicesWebService machineDevicesWebService)
            : base()
        {
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public InverterParametersData InverterParameters
        {
            get => this.inverterParameters;
            set => this.SetProperty(ref this.inverterParameters, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SetInvertersParamertersCommand =>
                   this.setInverterParamertersCommand
               ??
               (this.setInverterParamertersCommand = new DelegateCommand(
                async () => await this.SaveParametersAsync(), this.CanSave));

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.InverterParameters = null;
            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.IsWaitingForResponse = true;

            await base.OnAppearedAsync();

            if (this.Data is ISetVertimagConfiguration mainConfiguration)
            {
                this.parentConfiguration = mainConfiguration;
                this.InverterParameters = mainConfiguration.SelectedInverter;
            }

            this.IsWaitingForResponse = false;
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.setInverterParamertersCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.IsBusy;
        }

        private async Task SaveParametersAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                await this.parentConfiguration.BackupVertimagConfigurationParameters();

                await this.machineDevicesWebService.ProgramInverterAsync((byte)this.inverterParameters.InverterIndex, this.parentConfiguration.VertimagConfiguration);

                this.ShowNotification(Localized.Get("InstallationApp.InverterProgrammingStarted"), Services.Models.NotificationSeverity.Info);
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

        #endregion
    }
}
