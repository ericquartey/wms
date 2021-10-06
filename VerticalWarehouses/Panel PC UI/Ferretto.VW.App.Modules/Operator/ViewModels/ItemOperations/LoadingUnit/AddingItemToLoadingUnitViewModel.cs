using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Accessories.Interfaces;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class AddingItemToLoadingUnitViewModel : BaseOperatorViewModel, IOperationalContextViewModel
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly INavigationService navigationService;

        private bool acquiredLotValue;

        private bool acquiredSerialNumberValue;

        private DelegateCommand addItemCommand;

        private int compartmentId;

        private string expireDate;

        private bool expireDateVisibility;

        private double inputQuantity;

        private bool isAddItemButtonEnabled;

        private string itemDescription;

        private int itemId;

        private int loadingUnitId;

        private string lot;

        private bool lotVisibility;

        private string measureUnitTxt;

        private double quantityIncrement;

        private int? quantityTolerance;

        private string serialNumber;

        private bool serialNumberVisibility;

        private string titleText;

        #endregion

        #region Constructors

        public AddingItemToLoadingUnitViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            INavigationService navigationService,
            IDialogService dialogService)
           : base(PresentationMode.Operator)
        {
            this.Logger.Info("Ctor AddingItemToLoadingUnitViewModel");

            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemInventory.ToString();

        public ICommand AddItemCommand =>
                  this.addItemCommand
                  ??
                  (this.addItemCommand = new DelegateCommand(
                      async () => await this.AddItemToLoadingUnitAsync(),
                      this.CanAddItemButton));

        public string ExpireDate
        {
            get => this.expireDate;
            set
            {
                if (this.SetProperty(ref this.expireDate, value))
                {
                    // this.TriggerSearchAsync().GetAwaiter();  // Do not perform the searching routine
                }
            }
        }

        public bool ExpireDateVisibility
        {
            get => this.expireDateVisibility;
            set => this.SetProperty(ref this.expireDateVisibility, value, this.RaiseCanExecuteChanged);
        }

        public double InputQuantity
        {
            get => this.inputQuantity;
            set => this.SetProperty(ref this.inputQuantity, value, () =>
            {
                this.IsAddItemButtonEnabled = value > 0;
            });
        }

        public bool IsAddItemButtonEnabled
        {
            get => this.isAddItemButtonEnabled;
            protected set => this.SetProperty(ref this.isAddItemButtonEnabled, value, this.RaiseCanExecuteChanged);
        }

        public string ItemDescription
        {
            get => this.itemDescription;
            set => this.SetProperty(ref this.itemDescription, value, this.RaiseCanExecuteChanged);
        }

        public int LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value, this.RaiseCanExecuteChanged);
        }

        public string Lot
        {
            get => this.lot;
            set
            {
                if (this.SetProperty(ref this.lot, value))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.acquiredLotValue = false;
                    }
                }
            }
        }

        public bool LotVisibility
        {
            get => this.lotVisibility;
            set => this.SetProperty(ref this.lotVisibility, value, this.RaiseCanExecuteChanged);
        }

        public string MeasureUnitTxt
        {
            get => this.measureUnitTxt;
            set => this.SetProperty(ref this.measureUnitTxt, value, this.RaiseCanExecuteChanged);
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

        public string SerialNumber
        {
            get => this.serialNumber;
            set
            {
                if (this.SetProperty(ref this.serialNumber, value))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.acquiredSerialNumberValue = false;
                    }
                }
            }
        }

        public bool SerialNumberVisibility
        {
            get => this.serialNumberVisibility;
            set => this.SetProperty(ref this.serialNumberVisibility, value, this.RaiseCanExecuteChanged);
        }

        public string TitleText
        {
            get => this.titleText;
            set => this.SetProperty(ref this.titleText, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public async Task CommandUserActionAsync(UserActionEventArgs userAction)
        {
            if (userAction is null)
            {
                return;
            }

            if (userAction.IsReset)
            {
                return;
            }

            var readValue = userAction.Code;

            // Check and update: first Lot, then SerialNumber. Be careful about the order: do not change it
            if (this.lotVisibility && this.serialNumberVisibility)
            {
                if (this.acquiredLotValue && this.acquiredSerialNumberValue)
                {
                    this.acquiredLotValue = false;
                    this.acquiredSerialNumberValue = false;

                    this.Lot = string.Empty;
                    this.SerialNumber = string.Empty;
                }

                if (this.acquiredLotValue && !this.acquiredSerialNumberValue)
                {
                    this.SerialNumber = readValue;
                    this.acquiredSerialNumberValue = true;
                }

                if (!this.acquiredLotValue)
                {
                    this.Lot = readValue;
                    this.acquiredLotValue = true;
                }
            }

            if (this.lotVisibility && !this.serialNumberVisibility)
            {
                this.Lot = readValue;
                this.acquiredLotValue = true;
                this.acquiredSerialNumberValue = false;
            }

            if (!this.lotVisibility && this.serialNumberVisibility)
            {
                this.SerialNumber = readValue;
                this.acquiredLotValue = false;
                this.acquiredSerialNumberValue = true;
            }

            if (!this.lotVisibility && !this.serialNumberVisibility)
            {
                this.acquiredLotValue = false;
                this.acquiredSerialNumberValue = false;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.acquiredLotValue = false;
            this.acquiredSerialNumberValue = false;

            this.InputQuantity = 1;
            this.QuantityTolerance = 1;
            this.Lot = null;
            this.SerialNumber = null;
            this.ExpireDate = null;

            this.TitleText = Localized.Get("OperatorApp.AddingItemPageHeader");

            if (this.Data is ItemAddedToLoadingUnitDetail dataBundle)
            {
                this.itemId = dataBundle.ItemId;
                this.compartmentId = dataBundle.CompartmentId;

                this.LoadingUnitId = dataBundle.LoadingUnitId;
                this.ItemDescription = dataBundle.ItemDescription;
                this.MeasureUnitTxt = dataBundle.MeasureUnitTxt;
                this.QuantityTolerance = dataBundle.QuantityTolerance ?? 1;

                this.LotVisibility = await this.itemsWebService.IsItemHandledByLotAsync(this.itemId);
                this.SerialNumberVisibility = await this.itemsWebService.IsItemHandledBySerialNumberAsync(this.itemId);
            }

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.addItemCommand.RaiseCanExecuteChanged();
        }

        private async Task AddItemToLoadingUnitAsync()
        {
            this.ClearNotifications();

            try
            {
                this.Logger.Debug($"Immediate adding item {this.itemId} into loading unit {this.LoadingUnitId} ...");
                this.ShowNotification(Localized.Get("OperatorApp.ItemAdding"), Services.Models.NotificationSeverity.Info);

                await this.machineLoadingUnitsWebService.ImmediateAddItemAsync(
                    this.LoadingUnitId,
                    this.itemId,
                    this.InputQuantity,
                    this.compartmentId,
                    this.Lot,
                    this.SerialNumber);

                this.ShowNotification(Localized.Get("OperatorApp.ItemLoaded"), Services.Models.NotificationSeverity.Success);

                this.NavigationService.GoBack();
            }
            catch (Exception exc)
            {
                this.Logger.Debug($"Immediate adding item {this.itemId} into loading unit {this.LoadingUnitId} failed. Error: {exc}");
                this.ShowNotification(Localized.Get("OperatorApp.ItemAddingFailed"), Services.Models.NotificationSeverity.Error);
            }
        }

        private bool CanAddItemButton()
        {
            return this.isAddItemButtonEnabled;
        }

        #endregion
    }
}
