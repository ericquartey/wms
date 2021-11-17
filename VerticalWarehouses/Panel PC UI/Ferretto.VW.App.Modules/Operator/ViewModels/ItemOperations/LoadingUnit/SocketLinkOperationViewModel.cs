using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.Devices.LaserPointer;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class SocketLinkOperationViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IMachineMissionOperationsWebService missionOperationsWebService;

        private readonly INavigationService navigationService;

        private string compartmentText;

        private DelegateCommand confirmCommand;

        private double? inputQuantity;

        private bool isCompartmentValid;

        private int loadingUnitId;

        private string operationText;

        private double quantityIncrement;

        private int? quantityTolerance;

        private SocketLinkOperation socketLinkOperation;

        private object socketLinkOperationToken;

        private string titleText;

        #endregion

        #region Constructors

        public SocketLinkOperationViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            INavigationService navigationService,
            IMachineMissionOperationsWebService missionOperationsWebService,
            IDialogService dialogService)
           : base(PresentationMode.Operator)
        {
            this.Logger.Info("Ctor SocketLinkOperationViewModel");

            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            this.missionOperationsWebService = missionOperationsWebService ?? throw new ArgumentNullException(nameof(missionOperationsWebService));
        }

        #endregion

        #region Properties

        public string CompartmentText
        {
            get => this.compartmentText;
            set => this.SetProperty(ref this.compartmentText, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ConfirmCommand =>
                                  this.confirmCommand
                  ??
                  (this.confirmCommand = new DelegateCommand(
                      async () => await this.ConfirmCommandAsync(),
                      this.CanConfirmButton));

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value);
        }

        public bool IsCompartmentValid
        {
            get => this.isCompartmentValid;
            set => this.SetProperty(ref this.isCompartmentValid, value, this.RaiseCanExecuteChanged);
        }

        public int LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public string OperationText
        {
            get => this.operationText;
            set => this.SetProperty(ref this.operationText, value, this.RaiseCanExecuteChanged);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set
            {
                if (this.SetProperty(ref this.quantityTolerance, value))
                {
                    this.QuantityIncrement = Math.Pow(10, -this.quantityTolerance.Value);
                }
            }
        }

        public SocketLinkOperation SocketLinkOperation
        {
            get => this.socketLinkOperation;
            set => this.SetProperty(ref this.socketLinkOperation, value);
        }

        public string TitleText
        {
            get => this.titleText;
            set => this.SetProperty(ref this.titleText, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.LoadingUnitId = this.MachineService.Bay?.CurrentMission?.LoadUnitId ?? 0;
            this.TitleText = $"{Localized.Get("OperatorApp.LoadUnit")} {this.LoadingUnitId} {Localized.Get("OperatorApp.LoadingUnitInBay")}";

            if (this.Data is SocketLinkOperation dataBundle)
            {
                this.SocketLinkOperation = dataBundle;
                this.InputQuantity = this.SocketLinkOperation.RequestedQuantity;
                this.QuantityTolerance = 0;
                this.OperationText = $"{this.SocketLinkOperation.OperationType} {this.SocketLinkOperation.Id}";
                this.IsCompartmentValid = this.SocketLinkOperation.CompartmentX1Position.HasValue
                    && this.SocketLinkOperation.CompartmentX2Position.HasValue
                    && this.SocketLinkOperation.CompartmentY1Position.HasValue
                    && this.SocketLinkOperation.CompartmentY2Position.HasValue;
                if (this.IsCompartmentValid)
                {
                    this.CompartmentText = $"(x1: {this.SocketLinkOperation.CompartmentX1Position}) " +
                        $"(x2: {this.SocketLinkOperation.CompartmentX2Position}) " +
                        $"(y1: {this.SocketLinkOperation.CompartmentY1Position}) " +
                        $"(y2: {this.SocketLinkOperation.CompartmentY2Position})";
                }
            }

            this.socketLinkOperationToken = this.socketLinkOperationToken
                ??
                this.EventAggregator
                    .GetEvent<NotificationEventUI<SocketLinkOperationChangeMessageData>>()
                    .Subscribe(
                        this.OnSocketLinkOperationChanged,
                        ThreadOption.UIThread,
                        false);

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.confirmCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmButton()
        {
            return this.InputQuantity.HasValue
                && this.LoadingUnitId > 0;
        }

        private async Task ConfirmCommandAsync()
        {
            this.logger.Debug($"User requested to complete socket link operation '{this.SocketLinkOperation?.Id}' with quantity {this.InputQuantity.Value}.");
            await this.missionOperationsWebService.SocketLinkCompleteAsync(this.SocketLinkOperation?.Id, this.InputQuantity.Value, DateTimeOffset.UtcNow);
            this.NavigationService.GoBack();
        }

        private void OnSocketLinkOperationChanged(NotificationMessageUI<SocketLinkOperationChangeMessageData> e)
        {
            if (this.IsVisible)
            {
                var isOperation = !string.IsNullOrEmpty(e.Data.Id);
                if ((BayNumber)e.Data.BayNumber == this.MachineService.BayNumber)
                {
                    if (!isOperation)
                    {
                        this.logger.Debug($"Socket link operation '{this.SocketLinkOperation?.Id}' no more active!");
                        this.NavigationService.GoBack();
                    }
                }
            }
        }

        #endregion
    }
}
