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

        private readonly IMissionOperationsService missionOperationsService;

        private double? availableQuantity;

        private bool canInputQuantity;

        private bool canInputWastedDraperyQuantity;

        private DelegateCommand confirmDraperyItemCommand;

        private string draperyItemDescription;

        private double? inputQuantity;

        private string measureUnitTxt;

        private double missionRequestedQuantity;

        private double quantityIncrement;

        private int? quantityTolerance;

        private double wastedDraperyQuantity;

        #endregion

        #region Constructors

        public ItemDraperyConfirmViewModel(IMissionOperationsService missionOperationsService)
            : base(PresentationMode.Operator)
        {
            this.Logger.Debug("Ctor ItemDraperyConfirmViewModel!");

            this.missionOperationsService = missionOperationsService;
        }

        #endregion

        #region Properties

        public double? AvailableQuantity
        {
            get => this.availableQuantity;
            set => this.SetProperty(ref this.availableQuantity, value, this.RaiseCanExecuteChanged);
        }

        public string Barcode { get; set; }

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

        public bool IsPartiallyCompleteOperation { get; set; }

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
        }

        public int MissionId { get; set; }

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

            if (this.Data is ItemDraperyDataConfirm itemDraperyData)
            {
                this.MissionId = itemDraperyData.MissionId;

                this.CanInputQuantity = itemDraperyData.CanInputQuantity;
                this.QuantityIncrement = itemDraperyData.QuantityIncrement;
                this.QuantityTolerance = itemDraperyData.QuantityTolerance;
                this.DraperyItemDescription = itemDraperyData.ItemDescription;
                this.AvailableQuantity = itemDraperyData.AvailableQuantity;
                this.MissionRequestedQuantity = itemDraperyData.MissionRequestedQuantity;
                this.InputQuantity = itemDraperyData.InputQuantity;
                this.MeasureUnitTxt = itemDraperyData.MeasureUnitTxt;
                this.Barcode = itemDraperyData.Barcode;
                this.IsPartiallyCompleteOperation = itemDraperyData.IsPartiallyCompleteOperation;
            }
            else
            {
                this.CanInputQuantity = false;
                this.QuantityIncrement = 0.1;
                this.QuantityTolerance = 1;
                this.DraperyItemDescription = "No description available";
                this.AvailableQuantity = null;
                this.MissionRequestedQuantity = 0;
                this.InputQuantity = null;
                this.MeasureUnitTxt = string.Empty;
                this.Barcode = string.Empty;
                this.IsPartiallyCompleteOperation = false;
            }

            this.confirmDraperyItemCommand?.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.confirmDraperyItemCommand?.RaiseCanExecuteChanged();
        }

        private bool CanConfirmDraperyItemButton()
        {
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

                try
                {
                    var canComplete = await this.missionOperationsService.PartiallyCompleteAsync(this.MissionId, this.InputQuantity.Value);
                    if (!canComplete)
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.OperationCancelled"));
                        this.NavigationService.GoBack();
                    }
                }
                catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
                {
                    this.ShowNotification(ex);
                }
                finally
                {
                    this.IsWaitingForResponse = false;
                }
            }
            else
            {
                try
                {
                    this.IsWaitingForResponse = true;
                    this.ClearNotifications();

                    var canComplete = false;

                    if (this.Barcode != null)
                    {
                        this.ShowNotification(Localized.Get("OperatorApp.BarcodeOperationConfirmed") + this.Barcode, Services.Models.NotificationSeverity.Success);
                        canComplete = await this.missionOperationsService.CompleteAsync(this.MissionId, this.InputQuantity.Value, this.Barcode);
                    }
                    else
                    {
                        canComplete = await this.missionOperationsService.CompleteAsync(this.MissionId, this.InputQuantity.Value);
                    }

                    if (canComplete)
                    {
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
                }
            }
        }

        #endregion
    }
}
