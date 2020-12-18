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

        private InverterParametersData inverterParameters;

        private ISetVertimagConfiguration parentConfiguration;

        private InverterParameter selectedParameter;

        private DelegateCommand setInverterParamertersCommand;

        private DelegateCommand setParamerterCommand;

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

        private string GetInverterVersion(VertimagConfiguration vertimagConfiguration, byte inverterIndex)
        {
            foreach (var axe in vertimagConfiguration.Machine.Elevator.Axes)
            {
                if (!(axe.Inverter?.Parameters is null))
                {
                    var inverter = axe.Inverter;

                    if (inverterIndex == (byte)inverter.Index &&
                        inverter.Parameters.Any())
                    {
                        return inverter.Parameters.Single(s => s.Code == (short)InverterParameterId.SoftwareVersion).StringValue;
                    }
                }
            }

            foreach (var bay in vertimagConfiguration.Machine.Bays)
            {
                if (!(bay.Inverter?.Parameters is null))
                {
                    var inverter = bay.Inverter;

                    if (inverterIndex == (byte)inverter.Index &&
                        inverter.Parameters.Any())
                    {
                        return inverter.Parameters.Single(s => s.Code == (short)InverterParameterId.SoftwareVersion).StringValue;
                    }
                    if (!(bay.Shutter?.Inverter?.Parameters is null))
                    {
                        var inverterShutter = bay.Shutter.Inverter;

                        if (inverterIndex == (byte)inverter.Index &&
                            inverterShutter.Parameters.Any())
                        {
                            return inverter.Parameters.Single(s => s.Code == (short)InverterParameterId.SoftwareVersion).StringValue;
                        }
                    }
                }
            }

            return "";
        }

        private VertimagConfiguration GetUpdateConfiguration(VertimagConfiguration vertimagConfiguration, byte index, IEnumerable<InverterParameter> inverterParameters)
        {
            if (vertimagConfiguration.Machine.Elevator.Axes.Any(s => index == (byte)s.Inverter.Index))
            {
                var axe = vertimagConfiguration.Machine.Elevator.Axes.FirstOrDefault(s => index == (byte)s.Inverter.Index);
                axe.Inverter.Parameters = inverterParameters;
                //do
            }

            if (vertimagConfiguration.Machine.Bays.Any(s => index == (byte)s.Inverter.Index))
            {
                var bay = vertimagConfiguration.Machine.Bays.FirstOrDefault(s => index == (byte)s.Inverter.Index);
                bay.Inverter.Parameters = inverterParameters;
            }

            if (vertimagConfiguration.Machine.Bays.Any(s => index == (byte)s.Shutter.Inverter.Index))
            {
                var bay = vertimagConfiguration.Machine.Bays.FirstOrDefault(s => index == (byte)s.Shutter.Inverter.Index);
                bay.Shutter.Inverter.Parameters = inverterParameters;
            }

            return vertimagConfiguration;
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

        private async Task SaveParameterAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                await this.parentConfiguration.BackupVertimagConfigurationParameters();

                var parameterToList = new List<InverterParameter>();

                parameterToList.Add(this.SelectedParameter);

                var version = this.GetInverterVersion(this.parentConfiguration.VertimagConfiguration, this.inverterParameters.InverterIndex);

                var versionInverterParameter = new InverterParameter
                {
                    Code = (short)InverterParameterId.SoftwareVersion,
                    DataSet = 0,
                    Type = "String",
                    StringValue = version
                };

                parameterToList.Add(versionInverterParameter);

                var parameter = this.inverterParameters;

                parameter.Parameters = parameterToList;

                var config = this.GetUpdateConfiguration(this.parentConfiguration.VertimagConfiguration, this.inverterParameters.InverterIndex, parameterToList);

                await this.machineDevicesWebService.ProgramInverterAsync((byte)this.inverterParameters.InverterIndex, config);

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
