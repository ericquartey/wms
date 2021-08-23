using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ParametersInverterDetailsViewModel : BaseParameterInverterViewModel
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private DelegateCommand hardResetCommand;

        private Inverter inverterParameters;

        private SubscriptionToken inverterReadingMessageReceivedToken;

        private ISetVertimagInverterConfiguration parentConfiguration;

        private DelegateCommand readInverterCommand;

        private DelegateCommand refreshCommand;

        private DelegateCommand resetCommand;

        private InverterParameter selectedParameter;

        private DelegateCommand setInverterParamertersCommand;

        private DelegateCommand setParamerterCommand;

        #endregion

        #region Constructors

        public ParametersInverterDetailsViewModel(
            Services.IDialogService dialogService,
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService)
            : base(identityService)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public ICommand HardResetCommand =>
               this.hardResetCommand
               ??
               (this.hardResetCommand = new DelegateCommand(
                async () => await this.HardResetInverterAsync(), this.CanReset));

        public Inverter InverterParameters
        {
            get => this.inverterParameters;
            set => this.SetProperty(ref this.inverterParameters, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        public ICommand ReadInverterCommand =>
                   this.readInverterCommand
               ??
               (this.readInverterCommand = new DelegateCommand(
                   async () => await this.ReadInverterAsync(), this.CanRead));

        public ICommand RefreshCommand =>
               this.refreshCommand
               ??
               (this.refreshCommand = new DelegateCommand(
                async () => await this.RefreshAsync(), this.CanRefresh));

        public ICommand ResetCommand =>
                               this.resetCommand
               ??
               (this.resetCommand = new DelegateCommand(
                async () => await this.ResetInverterAsync(), this.CanReset));

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

            if (this.inverterReadingMessageReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterReadingMessageData>>().Unsubscribe(this.inverterReadingMessageReceivedToken);
                this.inverterReadingMessageReceivedToken?.Dispose();
                this.inverterReadingMessageReceivedToken = null;
            }

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.LoadData();

            this.SubscribeEvents();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.resetCommand?.RaiseCanExecuteChanged();
            this.hardResetCommand?.RaiseCanExecuteChanged();
            this.setInverterParamertersCommand?.RaiseCanExecuteChanged();
            this.setParamerterCommand?.RaiseCanExecuteChanged();
            this.readInverterCommand?.RaiseCanExecuteChanged();
            this.refreshCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsAdmin));
        }

        private bool CanRead()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin &&
                this.InverterParameters != null &&
                this.InverterParameters.Parameters.Any();
        }

        private bool CanRefresh()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private bool CanReset()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin;
        }

        private bool CanSave()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin &&
                this.InverterParameters != null &&
                this.InverterParameters.Parameters.Any();
        }

        private bool CanSaveParameter()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.sessionService.UserAccessLevel == UserAccessLevel.Admin &&
                this.selectedParameter != null &&
                !this.selectedParameter.IsReadOnly;
        }

        private async Task HardResetInverterAsync()
        {
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), "Hard reset inverter", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.ClearNotifications();
                    this.IsBusy = true;

                    this.parentConfiguration.BackupVertimagInverterConfigurationParameters();

                    await this.machineDevicesWebService.InverterHardResetAsync(this.inverterParameters);
                }
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

        private void LoadData()
        {
            this.IsBusy = true;

            if (this.Data is ISetVertimagInverterConfiguration mainConfiguration)
            {
                this.parentConfiguration = mainConfiguration;
                this.inverterParameters = mainConfiguration.SelectedInverter;
                this.inverterParameters.Parameters = this.inverterParameters.Parameters.OrderBy(s => s.Code).ThenBy(s => s.DataSet);
            }

            this.RaisePropertyChanged(nameof(this.InverterParameters));

            this.IsBusy = false;
        }

        private async Task OnInverterReadingMessageReceived(NotificationMessageUI<InverterReadingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    await this.RefreshAsync();
                    break;

                default:
                    break;
            }
        }

        private async Task ReadInverterAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                await this.machineDevicesWebService.ReadInverterAsync(this.inverterParameters.Index);
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

        private async Task RefreshAsync()
        {
            try
            {
                this.IsBusy = true;

                var inverters = await this.machineDevicesWebService.GetInvertersAsync();

                if (inverters.SingleOrDefault(s => s.Index == this.inverterParameters.Index) != null)
                {
                    this.inverterParameters = inverters.SingleOrDefault(s => s.Index == this.inverterParameters.Index);
                    this.inverterParameters.Parameters = this.inverterParameters.Parameters.OrderBy(s => s.Code).ThenBy(s => s.DataSet);
                }
                this.RaisePropertyChanged(nameof(this.InverterParameters));
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

        private async Task ResetInverterAsync()
        {
            try
            {
                var messageBoxResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), "Reset Inverter", DialogType.Question, DialogButtons.YesNo);
                if (messageBoxResult == DialogResult.Yes)
                {
                    this.ClearNotifications();
                    this.IsBusy = true;

                    this.parentConfiguration.BackupVertimagInverterConfigurationParameters();

                    await this.machineDevicesWebService.InverterResetAsync(this.inverterParameters);
                }
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

        private async Task SaveParameterAsync()
        {
            try
            {
                this.ClearNotifications();
                this.IsBusy = true;

                this.parentConfiguration.BackupVertimagInverterConfigurationParameters();

                var parameterToList = new List<InverterParameter>();

                parameterToList.Add(this.SelectedParameter);

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

        private void SubscribeEvents()
        {
            this.inverterReadingMessageReceivedToken = this.inverterReadingMessageReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterReadingMessageData>>()
                   .Subscribe(
                       async (m) => await this.OnInverterReadingMessageReceived(m),
                       ThreadOption.UIThread,
                       false);
        }

        #endregion
    }
}
