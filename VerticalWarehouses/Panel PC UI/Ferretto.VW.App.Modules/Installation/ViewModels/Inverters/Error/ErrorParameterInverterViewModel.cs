using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Modules.Installation.Interface;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ErrorParameterInverterViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineConfigurationWebService machineConfigurationWebService;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private SubscriptionToken inverterReadingMessageReceivedToken;

        private IEnumerable<Inverter> inverters;

        private bool isBusy;

        private DelegateCommand refreshCommand;

        private Inverter selectedInverter;

        private DelegateCommand<Inverter> showInverterParamertersCommand;

        #endregion

        #region Constructors

        public ErrorParameterInverterViewModel(
            IMachineIdentityWebService identityService,
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService,
            IMachineConfigurationWebService machineConfigurationWebService)
            : base(PresentationMode.Installer)
        {
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
            this.machineConfigurationWebService = machineConfigurationWebService ?? throw new ArgumentNullException(nameof(machineConfigurationWebService));
        }

        #endregion

        #region Properties

        public IEnumerable<Inverter> Inverters => this.inverters;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public ICommand RefreshCommand =>
                       this.refreshCommand
               ??
               (this.refreshCommand = new DelegateCommand(
                async () => await this.RefreshAsync(), this.CanRefresh));

        public Inverter SelectedInverter
        {
            get => this.selectedInverter;
            set => this.SetProperty(ref this.selectedInverter, value);
        }

        public ICommand ShowInverterParamertersCommand =>
                   this.showInverterParamertersCommand
               ??
               (this.showInverterParamertersCommand = new DelegateCommand<Inverter>(
                   this.ShowInverterParameters, this.CanShowInverterParameter));

        #endregion

        #region Methods

        public override void Disappear()
        {
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
            this.SubscribeEvents();

            await this.ReadData();

            await base.OnAppearedAsync();
        }

        private async Task ReadData()
        {
            try
            {
                this.IsBusy = true;

                this.inverters = await this.machineDevicesWebService.GetInvertersAsync();

                this.inverters = this.inverters.OrderBy(s => s.Index);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.RaisePropertyChanged(nameof(this.Inverters));
                this.IsBusy = false;
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
            this.showInverterParamertersCommand?.RaiseCanExecuteChanged();
            this.refreshCommand?.RaiseCanExecuteChanged();
        }

        private bool CanRefresh()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private bool CanShowInverterParameter(Inverter inverter)
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private async Task OnInverterReadingMessageReceived(NotificationMessageUI<InverterReadingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    this.inverters = await this.machineDevicesWebService.GetInvertersAsync();
                    this.RaisePropertyChanged(nameof(this.inverters));
                    break;

                default:
                    break;
            }
        }

        private async Task ReadAllInvertersAsync()
        {
            try
            {
                this.ClearNotifications();

                this.IsBusy = true;

                await this.machineDevicesWebService.ReadAllInvertersAsync();
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

                var result = await this.machineDevicesWebService.GetInvertersAsync();
                this.inverters = result.OrderBy(s => s.Index);

                this.RaisePropertyChanged(nameof(this.Inverters));
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

        private void ShowInverterParameters(Inverter inverterParametrers)
        {
            this.SelectedInverter = inverterParametrers;
            this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Inverters.ERRORPARAMETERSINVERTERDETAILS,
                data: this,
                trackCurrentView: true);
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
