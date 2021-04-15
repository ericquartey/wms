using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Modules.Operator.Models;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Microsoft.AspNetCore.Http;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    [Warning(WarningsArea.Picking)]
    public class ItemSearchUnitsViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IAuthenticationService authenticationService;

        private readonly IMachineItemsWebService itemsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private readonly IWmsDataProvider wmsDataProvider;

        private DelegateCommand callLoadingUnitCommand;

        private DelegateCommand checkProductCommand;

        private ItemInfo item;

        private ObservableCollection<Compartment> itemUnits;

        private Compartment selectedItemUnits;

        #endregion

        #region Constructors

        public ItemSearchUnitsViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            IMachineItemsWebService itemsWebService,
            IAuthenticationService authenticationService,
            IWmsDataProvider wmsDataProvider
            )
            : base(PresentationMode.Operator)
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.itemsWebService = itemsWebService ?? throw new ArgumentNullException(nameof(itemsWebService));
            this.authenticationService = authenticationService ?? throw new ArgumentNullException(nameof(authenticationService));
            this.wmsDataProvider = wmsDataProvider ?? throw new ArgumentNullException(nameof(wmsDataProvider));
        }

        #endregion

        #region Properties

        public string ActiveContextName => OperationalContext.ItemsSearch.ToString();

        public ICommand CheckProductCommand =>
            this.checkProductCommand
            ??
            (this.checkProductCommand = new DelegateCommand(
                async () => await this.CheckProductAsync(),
                this.CanCheckProduct));

        public ItemInfo Item
        {
            get => this.item;
            set
            {
                if (value is null)
                {
                    this.RaisePropertyChanged();
                    return;
                }

                this.SetProperty(ref this.item, value);
            }
        }

        public ObservableCollection<Compartment> ItemUnits
        {
            get => this.itemUnits;
            set => this.SetProperty(ref this.itemUnits, value, this.RaiseCanExecuteChanged);
        }

        public ICommand LoadUnitCallCommand =>
            this.callLoadingUnitCommand
            ??
            (this.callLoadingUnitCommand = new DelegateCommand(
                async () => await this.CallLoadingUnitAsync(),
                this.CanCallLoadingUnit));

        public Compartment SelectedItemUnits
        {
            get => this.selectedItemUnits;
            set => this.SetProperty(ref this.selectedItemUnits, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public async Task CallLoadingUnitAsync()
        {
            this.Logger.Debug($"CallLoadingUnitAsync: LoadingUnitId {this.SelectedItemUnits?.LoadingUnitId} ");
            if (this.SelectedItemUnits?.LoadingUnitId == null)
            {
                this.ShowNotification(Resources.Localized.Get("General.IdLoadingUnitNotExists"), Services.Models.NotificationSeverity.Warning);
                return;
            }

            try
            {
                this.IsWaitingForResponse = true;

                await this.machineLoadingUnitsWebService.MoveToBayAsync(this.SelectedItemUnits.LoadingUnitId, this.authenticationService.UserName);

                this.ShowNotification(string.Format(Resources.Localized.Get("ServiceMachine.LoadingUnitSuccessfullyRequested"), this.SelectedItemUnits.LoadingUnitId), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                if (ex is MasWebApiException webEx
                    && webEx.StatusCode == StatusCodes.Status403Forbidden)
                {
                    this.ShowNotification(Resources.Localized.Get("General.ForbiddenOperation"), Services.Models.NotificationSeverity.Error);
                }
                else
                {
                    this.ShowNotification(ex);
                }
            }
            finally
            {
                this.SelectedItemUnits = null;
                this.IsWaitingForResponse = false;
            }
        }

        public async Task CheckProductAsync()
        {
            try
            {
                if (this.SelectedItemUnits?.Id != null)
                {
                    await this.wmsDataProvider.CheckAsync(
                        this.Item.Id,
                        this.SelectedItemUnits.Id,
                        this.SelectedItemUnits.Lot,
                        this.SelectedItemUnits.Sub1,
                        this.authenticationService.UserName);

                    this.ShowNotification(
                        Resources.Localized.Get("OperatorApp.OperationConfirmed"),
                        Services.Models.NotificationSeverity.Success);
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        public override void Disappear()
        {
            this.Item = null;

            this.ItemUnits = null;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.NoteEnabled = true;
            this.RaisePropertyChanged(nameof(this.NoteEnabled));

            this.IsBackNavigationAllowed = true;

            this.Item = this.Data as ItemInfo;
            var compartments = await this.itemsWebService.GetCompartmentsAsync(this.Item.Id);

            // filter ItemUnits by lot and serialNumber
            this.ItemUnits = new ObservableCollection<Compartment>();

            foreach (var compartment in compartments)
            {
                var itemsCompartments = await this.machineLoadingUnitsWebService.GetCompartmentsAsync(compartment.LoadingUnitId);
                //if (this.Item.Lot != null || this.Item.SerialNumber != null)
                //{
                var filteredCompartments = itemsCompartments.Where(c => (c.ItemId == this.Item.Id)
                    && ((this.Item.Lot == null && this.Item.SerialNumber == null)
                        || (this.Item.Lot != null && c.Lot == this.Item.Lot)
                        || (this.Item.SerialNumber != null && c.ItemSerialNumber == this.Item.SerialNumber))
                    );

                if (filteredCompartments != null)
                {
                    foreach (var filteredCompartment in filteredCompartments)
                    {
                        var newCompartment = new Compartment()
                        {
                            LoadingUnitId = compartment.LoadingUnitId,
                            Stock = compartment.Stock,
                            Id = compartment.Id,
                            Lot = filteredCompartment.Lot,
                            Sub1 = filteredCompartment.ItemSerialNumber,
                        };
                        this.ItemUnits.Add(newCompartment);
                    }
                }
                //}
                //else
                //{
                //    this.ItemUnits.Add(compartment);
                //}
            }

            this.RaisePropertyChanged(nameof(this.ItemUnits));

            if (this.ItemUnits.Any())
            {
                this.SelectedItemUnits = this.ItemUnits.FirstOrDefault();
                this.RaisePropertyChanged(nameof(this.SelectedItemUnits));
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.callLoadingUnitCommand?.RaiseCanExecuteChanged();

            this.checkProductCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCallLoadingUnit()
        {
            return
                this.SelectedItemUnits?.LoadingUnitId != null
                &&
                !this.IsWaitingForResponse;
        }

        private bool CanCheckProduct()
        {
            return
                this.SelectedItemUnits?.LoadingUnitId != null
                &&
                !this.IsWaitingForResponse;
        }

        #endregion
    }
}
