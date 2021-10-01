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
    public class AddingItemToLoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly Services.IDialogService dialogService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly INavigationService navigationService;

        private DelegateCommand addItemCommand;

        private int compartmentId;

        private string expireDate;

        private bool expireDateVisibility;

        private double inputQuantity;

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
            set => this.SetProperty(ref this.inputQuantity, value, this.RaiseCanExecuteChanged);
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
                    // this.TriggerSearchAsync().GetAwaiter();  // Do not perform the searching routine
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
            set => this.SetProperty(ref this.quantityIncrement, value, this.RaiseCanExecuteChanged);
        }

        public int? QuantityTolerance
        {
            get => this.quantityTolerance;
            set => this.SetProperty(ref this.quantityTolerance, value, this.RaiseCanExecuteChanged);
        }

        public string SerialNumber
        {
            get => this.serialNumber;
            set
            {
                if (this.SetProperty(ref this.serialNumber, value))
                {
                    // this.TriggerSearchAsync().GetAwaiter();  // Do not perform the searching routine
                }
            }
        }

        public bool SerialNumberVisibility
        {
            get => this.serialNumberVisibility;
            set => this.SetProperty(ref this.serialNumberVisibility, value, this.RaiseCanExecuteChanged);
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
            if (this.LotVisibility)
            {
                if (string.IsNullOrEmpty(this.Lot))
                {
                    this.Lot = readValue;
                }
            }

            if (this.SerialNumberVisibility)
            {
                if (string.IsNullOrEmpty(this.SerialNumber))
                {
                    this.SerialNumber = readValue;
                }
            }
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.InputQuantity = 0;
            this.QuantityTolerance = 1;
            this.QuantityIncrement = 0.1;
            this.Lot = null;
            this.SerialNumber = null;
            this.ExpireDate = null;

            if (this.Data is ItemAddedToLoadingUnitDetail dataBundle)
            {
                this.itemId = dataBundle.ItemId;
                this.compartmentId = dataBundle.CompartmentId;

                this.LoadingUnitId = dataBundle.LoadingUnitId;
                this.ItemDescription = dataBundle.ItemDescription;
                this.MeasureUnitTxt = dataBundle.MeasureUnitTxt;
                this.QuantityIncrement = dataBundle.QuantityIncrement;
                this.QuantityTolerance = dataBundle.QuantityTolerance;

                this.LotVisibility = await this.itemsWebService.IsItemHandledByLotAsync(this.itemId);
                this.SerialNumberVisibility = await this.itemsWebService.IsItemHandledBySerialNumberAsync(this.itemId);
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();
        }

        private async Task AddItemToLoadingUnitAsync()
        {
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

        private bool CanAddItemButton()
        {
            return this.InputQuantity > 0;
        }

        #endregion
    }
}
