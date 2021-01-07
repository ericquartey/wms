using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.InverterDriver.Contracts;
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

        private Inverter inverterParameters;

        private ISetVertimagInverterConfiguration parentConfiguration;

        private InverterParameter selectedParameter;

        private DelegateCommand setInverterParamertersCommand;

        private DelegateCommand setParamerterCommand;

        #endregion

        #region Constructors

        public ParametersInverterDetailsViewModel(
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService)
            : base(identityService)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public Inverter InverterParameters
        {
            get => this.inverterParameters;
            set => this.SetProperty(ref this.inverterParameters, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        public InverterParameter SelectedParameter
        {
            get => this.selectedParameter;
            set => this.SetProperty(ref this.selectedParameter, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SetInvertersParamertersCommand =>
               this.setInverterParamertersCommand
               ??
               (this.setInverterParamertersCommand = new DelegateCommand(
                async () => await this.SaveParametersAsync(), this.CanSave));

        public ICommand SetParamerterCommand =>
               this.setParamerterCommand
               ??
               (this.setParamerterCommand = new DelegateCommand(
                async () => await this.SaveParameterAsync(), this.CanSaveParameter));

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
            this.setParamerterCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsAdmin));
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin &&
                this.InverterParameters != null &&
                this.InverterParameters.Parameters.Any();
        }

        private bool CanSaveParameter()
        {
            return !this.IsBusy &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin &&
                this.selectedParameter != null &&
                !this.selectedParameter.IsReadOnly;
        }

        private void LoadData()
        {
            this.IsWaitingForResponse = true;

            if (this.Data is ISetVertimagInverterConfiguration mainConfiguration)
            {
                this.parentConfiguration = mainConfiguration;
                this.InverterParameters = mainConfiguration.SelectedInverter;
            }

            this.RaisePropertyChanged(nameof(this.InverterParameters));

            this.IsWaitingForResponse = false;
        }

        private async Task SaveParameterAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                this.parentConfiguration.BackupVertimagInverterConfigurationParameters();

                var parameterToList = new List<InverterParameter>();

                parameterToList.Add(this.SelectedParameter);

                var versionInverterParameter = new InverterParameter
                {
                    Code = (short)InverterParameterId.SoftwareVersion,
                    DataSet = 0,
                    IsReadOnly = true,
                    Type = "String",
                    StringValue = this.inverterParameters.Parameters.Single(s => s.Code == (short)InverterParameterId.SoftwareVersion).StringValue
                };

                parameterToList.Add(versionInverterParameter);

                var parameter = this.inverterParameters;

                Inverter inverter = new Inverter
                {
                    Id = this.inverterParameters.Id,
                    Index = this.inverterParameters.Index,
                    IpAddress = this.inverterParameters.IpAddress,
                    Parameters = parameterToList,
                    TcpPort = this.inverterParameters.TcpPort,
                    Type = this.inverterParameters.Type
                };

                await this.machineDevicesWebService.ProgramInverterAsync(inverter);
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

        private async Task SaveParametersAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                this.parentConfiguration.BackupVertimagInverterConfigurationParameters();

                await this.machineDevicesWebService.ProgramInverterAsync(this.inverterParameters);
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
