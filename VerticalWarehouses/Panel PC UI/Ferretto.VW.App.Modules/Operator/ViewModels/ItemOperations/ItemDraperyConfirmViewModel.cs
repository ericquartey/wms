using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ItemDraperyConfirmViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService loadingUnitsWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private readonly IMissionOperationsService missionOperationsService;

        private double? availableQuantity;

        private bool canInputQuantity;

        private bool canInputWastedDraperyQuantity;

        private DelegateCommand confirmDraperyItemCommand;

        private string draperyItemDescription;

        private bool forceDisablingConfirmButton;

        private double? inputQuantity;

        private bool isFullyRequested;

        private string measureUnitTxt;

        private double missionRequestedQuantity;

        private double quantityIncrement;

        private int? quantityTolerance;

        private double wastedDraperyQuantity;

        #endregion

        #region Constructors

        public ItemDraperyConfirmViewModel(IMissionOperationsService missionOperationsService,
            IMachineItemsWebService itemsWebService,
            IMachineLoadingUnitsWebService loadingUnitsWebService,
            IMachineIdentityWebService machineIdentityWebService,
            IDialogService dialogService)
            : base(PresentationMode.Operator)
        {
            this.missionOperationsService = missionOperationsService ?? throw new ArgumentNullException(nameof(missionOperationsService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.loadingUnitsWebService = loadingUnitsWebService ?? throw new ArgumentNullException(nameof(loadingUnitsWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public double? AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value, this.RaiseCanExecuteChanged);
        }

        public string Barcode { get; set; }

        public int BarcodeLenght { get; set; }

        public bool CanInputQuantity
        {
            get => this.canInputQuantity;
            protected set => this.SetProperty(ref this.canInputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool CanInputWastedDraperyQuantity
        {
            get => this.canInputWastedDraperyQuantity;
            protected set => this.SetProperty(ref this.canInputWastedDraperyQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool CloseLine { get; set; }

        public ICommand ConfirmDraperyItemCommand =>
                          this.confirmDraperyItemCommand
                  ??
                  (this.confirmDraperyItemCommand = new DelegateCommand(
                      async () => await this.ConfirmOperationAsync(),
                      this.CanConfirmDraperyItemButton));

        public string DraperyItemDescription
        {
            get => this.draperyItemDescription;
            set => this.SetProperty(ref this.draperyItemDescription, value, this.RaiseCanExecuteChanged);
        }

        public double? InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
        }

        public bool IsCurrentDraperyItemFullyRequested
        {
            get => this.isFullyRequested;
            set => this.SetProperty(ref this.isFullyRequested, value, this.RaiseCanExecuteChanged);
        }

        public bool IsPartiallyCompleteOperation { get; set; }

        public int ItemId { get; set; }

        public int LoadingUnitId { get; set; }

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
        }

        public int MissionId { get; set; }

        public MissionOperationType MissionOperationType { get; set; }

        public double MissionRequestedQuantity
        {
            get => this.missionRequestedQuantity;
            set => this.SetProperty(ref this.missionRequestedQuantity, value, this.RaiseCanExecuteChanged);
        }

        public double QuantityIncrement
        {
            get => this.quantityIncrement;
            set => this.SetProperty(ref this.quantityIncrement, value, this.RaiseCanExecuteChanged);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set => this.SetProperty(ref this.quantityTolerance, value, this.RaiseCanExecuteChanged);
        }

        public double WastedDraperyQuantity
        {
            get => this.wastedDraperyQuantity;
            set => this.SetProperty(ref this.wastedDraperyQuantity, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            this.WastedDraperyQuantity = 0;
            this.CanInputWastedDraperyQuantity = true;
            this.QuantityTolerance = 1;
            this.QuantityIncrement = 0.1;

            this.forceDisablingConfirmButton = false;

            if (this.Data is ItemDraperyDataConfirm itemDraperyData)
            {
                this.MissionId = itemDraperyData.MissionId;

                this.ItemId = itemDraperyData.ItemId;
                this.LoadingUnitId = itemDraperyData.LoadingUnitId;
                this.MissionOperationType = itemDraperyData.MissionOperationType;
                this.CanInputQuantity = itemDraperyData.CanInputQuantity;
                this.QuantityIncrement = itemDraperyData.QuantityIncrement;
                this.QuantityTolerance = itemDraperyData.QuantityTolerance;
                this.DraperyItemDescription = itemDraperyData.ItemDescription;
                this.AvailableQuantity = itemDraperyData.AvailableQuantity;
                this.MissionRequestedQuantity = itemDraperyData.MissionRequestedQuantity;
                this.InputQuantity = itemDraperyData.InputQuantity;
                this.MeasureUnitTxt = itemDraperyData.MeasureUnitTxt;
                this.Barcode = itemDraperyData.Barcode;
                this.BarcodeLenght = itemDraperyData.BarcodeLength;
                this.IsPartiallyCompleteOperation = itemDraperyData.IsPartiallyCompleteOperation;
                this.IsCurrentDraperyItemFullyRequested = itemDraperyData.FullyRequested;
                this.CloseLine = itemDraperyData.CloseLine;
            }
            else
            {
                this.ItemId = -1;
                this.LoadingUnitId = -1;
                this.MissionOperationType = MissionOperationType.NotSpecified;
                this.CanInputQuantity = false;
                this.QuantityIncrement = 0.1;
                this.QuantityTolerance = 1;
                this.DraperyItemDescription = "No description available";
                this.AvailableQuantity = null;
                this.MissionRequestedQuantity = 0;
                this.InputQuantity = null;
                this.MeasureUnitTxt = string.Empty;
                this.Barcode = string.Empty;
                this.BarcodeLenght = 0;
                this.IsPartiallyCompleteOperation = false;
                this.IsCurrentDraperyItemFullyRequested = false;
                this.CloseLine = true;
            }

            this.confirmDraperyItemCommand?.RaiseCanExecuteChanged();
        }

        public async Task UpdateWeight(int loadingUnitId, double quantity, int? itemWeight, MissionOperationType missionOperationType)
        {
            if (itemWeight != null && itemWeight != 0)
            {
                var loadingUnit = await this.loadingUnitsWebService.GetByIdAsync(loadingUnitId);

                var grossWeight = default(double);
                if (missionOperationType == MissionOperationType.Put)
                {
                    grossWeight = loadingUnit.GrossWeight + (itemWeight.Value * quantity / 1000);
                }
                else if (missionOperationType == MissionOperationType.Pick)
                {
                    grossWeight = loadingUnit.GrossWeight - (itemWeight.Value * quantity / 1000);
                }

                //this.logger.Debug($"Set weight {grossWeight:0.00} to LoadUnit {loadingUnitId} difference {quantity} unit weight {itemWeight.Value} original weight {loadingUnit.GrossWeight:0.00}");
                await this.loadingUnitsWebService.SetLoadingUnitWeightAsync(loadingUnitId, grossWeight);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmDraperyItemCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmDraperyItemButton()
        {
            // force disabling the button
            if (this.forceDisablingConfirmButton)
            {
                return false;
            }

            bool canConfirm;
            if (!this.InputQuantity.HasValue ||
                !this.AvailableQuantity.HasValue)
            {
                canConfirm = false;
            }
            else
            {
                canConfirm = this.InputQuantity.Value + this.wastedDraperyQuantity < this.AvailableQuantity.Value;
            }

            return canConfirm;
        }

        private async Task ConfirmOperationAsync()
        {
            if (this.IsPartiallyCompleteOperation)
            {
                this.IsWaitingForResponse = true;
                this.forceDisablingConfirmButton = true;

                try
                {
                    bool canComplete;

                    var item = await this.itemsWebService.GetByIdAsync(this.ItemId);
                    var loadingUnitId = this.LoadingUnitId;
                    var type = this.MissionOperationType;

                    this.CanConfirmDraperyItemButton();

                    // Show the confirm message dialog, if requested
                    var isRequestConfirm = await this.machineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                    if (isRequestConfirm)
                    {
                        var isLastMissionOnCurrentLoadingUnit = await this.missionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionId);
                        if (isLastMissionOnCurrentLoadingUnit)
                        {
                            var messageBoxResult = this.dialogService.ShowMessage(
                                Localized.Get("InstallationApp.ConfirmationOperation"),
                                Localized.Get("InstallationApp.ConfirmationOperation"),
                                DialogType.Question,
                                DialogButtons.OK);
                            if (messageBoxResult is DialogResult.OK)
                            {
                                // go away...
                            }
                        }
                    }

                    if (this.CloseLine)
                    {
                        canComplete = await this.missionOperationsService.PartiallyCompleteAsync(this.MissionId, this.InputQuantity.Value, this.WastedDraperyQuantity, null, null, null);
                    }
                    else
                    {
                        canComplete = await this.missionOperationsService.CompleteAsync(this.MissionId, this.InputQuantity.Value, this.Barcode, this.WastedDraperyQuantity);
                    }

                    if (canComplete)
                    {
                        await this.UpdateWeight(loadingUnitId, this.InputQuantity.Value, item.AverageWeight, type);

                        this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                    }
                    else
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                    }

                    this.NavigationService.GoBack();
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsWaitingForResponse = false;
                    this.forceDisablingConfirmButton = false;
                }
            }
            else
            {
                this.forceDisablingConfirmButton = true;

                try
                {
                    this.IsWaitingForResponse = true;
                    this.ClearNotifications();

                    var canComplete = false;

                    var item = await this.itemsWebService.GetByIdAsync(this.ItemId);

                    var loadingUnitId = this.LoadingUnitId;
                    var type = this.MissionOperationType;

                    this.CanConfirmDraperyItemButton();

                    // Show the confirm message dialog, if requested
                    var isRequestConfirm = await this.machineIdentityWebService.IsRequestConfirmForLastOperationOnLoadingUnitAsync();
                    if (isRequestConfirm)
                    {
                        var isLastMissionOnCurrentLoadingUnit = await this.missionOperationsService.IsLastWmsMissionForCurrentLoadingUnitAsync(this.MissionId);
                        if (isLastMissionOnCurrentLoadingUnit)
                        {
                            var messageBoxResult = this.dialogService.ShowMessage(
                                Localized.Get("InstallationApp.ConfirmationOperation"),
                                Localized.Get("InstallationApp.ConfirmationOperation"),
                                DialogType.Question,
                                DialogButtons.OK);
                            if (messageBoxResult is DialogResult.OK)
                            {
                                // go away...
                            }
                        }
                    }

                    if (this.Barcode != null)
                    {
                        //x this.ShowNotification(Localized.Get("OperatorApp.BarcodeOperationConfirmed") + this.Barcode, Services.Models.NotificationSeverity.Success);
                        canComplete = await this.missionOperationsService.CompleteAsync(this.MissionId, this.InputQuantity.Value, this.Barcode, this.WastedDraperyQuantity);
                    }
                    else
                    {
                        canComplete = await this.missionOperationsService.CompleteAsync(this.MissionId, this.InputQuantity.Value);
                    }

                    if (canComplete)
                    {
                        if (this.Barcode != null && this.BarcodeLenght > 0 && this.Barcode.Length == this.BarcodeLenght)
                        {
                            await this.UpdateWeight(loadingUnitId, 1, item.AverageWeight, type);
                        }
                        else
                        {
                            await this.UpdateWeight(loadingUnitId, this.InputQuantity.Value, item.AverageWeight, type);
                        }

                        this.ShowNotification(Localized.Get("OperatorApp.OperationConfirmed"));
                    }
                    else
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                        //this.NavigationService.GoBack();
                    }

                    //this.NavigationService.GoBackTo(
                    //    nameof(Utils.Modules.Operator),
                    //    Utils.Modules.Operator.ItemOperations.WAIT);

                    this.forceDisablingConfirmButton = false;

                    this.NavigationService.GoBack();
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    // Do not enable the interface. Wait for a new notification to arrive.
                    this.IsWaitingForResponse = false;
                    this.forceDisablingConfirmButton = false;
                }
            }

            this.CanConfirmDraperyItemButton();
        }

        #endregion
    }
}
