using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class ErrorParametersInverterDetailsViewModel : BaseMainViewModel
    {
        #region Fields

        public static readonly IList<short> ang = new ReadOnlyCollection<short>(new List<short> { 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322, 323, 324, 325, 362, 363, 330, 331, 332, 333, 334, 335, 336,
                                                                                                  337, 338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 349, 350, 351, 352, 353, 354, 355, 356, 357, 358, 359, 360, 361, 403,
                                                                                                  259, 269, 273, 1247, 275, 249, 244, 245, 222, 223, 255, 256, 250, 243, 277, 251, 253, 228, 282, 283, 229, 254, 257, 266, 242, 237,
                                                                                                  231, 232, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 301, 302, 1121, 29, 0, 1, 12, 16});

        private readonly Services.IDialogService dialogService;

        private readonly IMachineDevicesWebService machineDevicesWebService;

        private readonly ISessionService sessionService;

        private SubscriptionToken inverterParameterReceivedToken;

        private Inverter inverterParameters;

        private SubscriptionToken inverterReadingMessageReceivedToken;

        private bool isBusy;

        private DelegateCommand readInverterCommand;

        private DelegateCommand refreshCommand;

        private InverterParameter selectedParameter;

        #endregion

        #region Constructors

        public ErrorParametersInverterDetailsViewModel(
            Services.IDialogService dialogService,
            ISessionService sessionService,
            IMachineDevicesWebService machineDevicesWebService)
            : base(PresentationMode.Installer)
        {
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
            this.machineDevicesWebService = machineDevicesWebService ?? throw new ArgumentNullException(nameof(machineDevicesWebService));
        }

        #endregion

        #region Properties

        public override EnableMask EnableMask => EnableMask.Any;

        public Inverter InverterParameters
        {
            get => this.inverterParameters;
            set => this.SetProperty(ref this.inverterParameters, value, this.RaiseCanExecuteChanged);
        }

        public bool IsAdmin => this.sessionService.UserAccessLevel == UserAccessLevel.Admin;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value, this.RaiseCanExecuteChanged);
        }

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

        public InverterParameter SelectedParameter
        {
            get => this.selectedParameter;
            set => this.SetProperty(ref this.selectedParameter, value, this.RaiseCanExecuteChanged);
        }

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

            if (this.inverterParameterReceivedToken != null)
            {
                this.EventAggregator.GetEvent<NotificationEventUI<InverterParametersMessageData>>().Unsubscribe(this.inverterParameterReceivedToken);
                this.inverterParameterReceivedToken?.Dispose();
                this.inverterParameterReceivedToken = null;
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

            this.readInverterCommand?.RaiseCanExecuteChanged();
            this.refreshCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.IsAdmin));
        }

        private bool CanRead()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered &&
                this.InverterParameters != null &&
                this.InverterParameters.Parameters.Any();
        }

        private bool CanRefresh()
        {
            return !this.IsBusy &&
                this.MachineService.MachinePower <= MachinePowerState.Unpowered;
        }

        private void LoadData()
        {
            this.IsBusy = true;

            if (this.Data is Inverter mainConfiguration)
            {
                this.inverterParameters = mainConfiguration;
                var errorParameter = new List<InverterParameter>();

                if (this.inverterParameters.Type == InverterType.Ang)
                {
                    foreach (var parameter in this.inverterParameters.Parameters)
                    {
                        if (ang.Any(s => s == parameter.Code))
                        {
                            errorParameter.Add(parameter);
                        }
                    }
                }
                else if (this.inverterParameters.Type == InverterType.Agl)
                {
                }
                else if (this.inverterParameters.Type == InverterType.Acu)
                {
                }

                this.inverterParameters.Parameters = errorParameter.OrderBy(s => s.Code).ThenBy(s => s.DataSet);
            }

            this.RaisePropertyChanged(nameof(this.InverterParameters));

            this.IsBusy = false;
        }

        private void OnInverterParameterReceived(NotificationMessageUI<InverterParametersMessageData> message)
        {
            if (message.Status == CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd)
            {
                this.ShowNotification(message.Data.ToString(), Services.Models.NotificationSeverity.Info);
            }
        }

        private void OnInverterReadingMessageReceived(NotificationMessageUI<InverterReadingMessageData> message)
        {
            switch (message.Status)
            {
                case CommonUtils.Messages.Enumerations.MessageStatus.OperationEnd:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingSuccessfullyEnded"), Services.Models.NotificationSeverity.Success);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationError:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingEndedErrors"), Services.Models.NotificationSeverity.Error);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStart:
                    this.IsBusy = true;
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingStarted"), Services.Models.NotificationSeverity.Info);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStop:
                    this.IsBusy = false;
                    this.ShowNotification(Localized.Get("InstallationApp.InvertersReadingStopped"), Services.Models.NotificationSeverity.Warning);
                    break;

                case CommonUtils.Messages.Enumerations.MessageStatus.OperationStepEnd:
                    this.ShowNotification(Localized.Get("InstallationApp.InverterReadingNext"), Services.Models.NotificationSeverity.Info);
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

                await this.machineDevicesWebService.ReadInverterParameterAsync(this.inverterParameters);
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
                    var errorParameter = new List<InverterParameter>();

                    if (this.inverterParameters.Type == InverterType.Ang)
                    {
                        foreach (var parameter in this.inverterParameters.Parameters)
                        {
                            if (ang.Any(s => s == parameter.Code))
                            {
                                errorParameter.Add(parameter);
                            }
                        }
                    }
                    else if (this.inverterParameters.Type == InverterType.Agl)
                    {
                    }
                    else if (this.inverterParameters.Type == InverterType.Acu)
                    {
                    }

                    this.inverterParameters.Parameters = errorParameter.OrderBy(s => s.Code).ThenBy(s => s.DataSet);
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

        private void SubscribeEvents()
        {
            this.inverterReadingMessageReceivedToken = this.inverterReadingMessageReceivedToken
               ?? this.EventAggregator
                   .GetEvent<NotificationEventUI<InverterReadingMessageData>>()
                   .Subscribe(
                       (m) => this.OnInverterReadingMessageReceived(m),
                       ThreadOption.UIThread,
                       false);

            this.inverterParameterReceivedToken = this.inverterParameterReceivedToken
                ?? this.EventAggregator
                    .GetEvent<NotificationEventUI<InverterParametersMessageData>>()
                    .Subscribe(
                        (m) => this.OnInverterParameterReceived(m),
                        ThreadOption.UIThread,
                        false);
        }

        #endregion
    }
}
