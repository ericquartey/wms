using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
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

        private readonly ISessionService sessionService;

        private InverterParametersData inverterParameters;

        private ISetVertimagConfiguration parentConfiguration;

        private object selectedParameter;

        private DelegateCommand setInverterParamertersCommand;

        #endregion

        #region Constructors

        public ParametersInverterDetailsViewModel(
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService)
            : base()
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public InverterParametersData InverterParameters
        {
            get => this.inverterParameters;
            set => this.SetProperty(ref this.inverterParameters, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        public object SelectedParameter
        {
            get => this.selectedParameter;
            set => this.SetProperty(ref this.selectedParameter, value, this.RaiseCanExecuteChanged);
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
            await base.OnAppearedAsync();

            this.LoadData();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.setInverterParamertersCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsAdmin));
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        private void LoadData()
        {
            this.IsWaitingForResponse = true;

            if (this.Data is ISetVertimagConfiguration mainConfiguration)
            {
                this.parentConfiguration = mainConfiguration;
                this.InverterParameters = mainConfiguration.SelectedInverter;
            }

            this.RaisePropertyChanged(nameof(this.InverterParameters));

            this.IsWaitingForResponse = false;
        }

        private async Task SaveParametersAsync()
        {
            try
            {
                ////fix selected item changed
                //var formattedParameters = this.inverterParameters.Parameters as List<InverterParameter>;

                //if (formattedParameters.Where(s => s.Code == this.selectedParameter.Code).Select(s => s.StringValue).FirstOrDefault() != this.selectedParameter.StringValue)
                //{
                //    formattedParameters.Where(s => s.Code == this.selectedParameter.Code).FirstOrDefault().StringValue = this.selectedParameter.StringValue;
                //}

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
