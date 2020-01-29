using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class BaseLoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly ILoadingUnitsWmsWebService loadingUnitsWmsWebService;

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private MAS.AutomationService.Contracts.Bay bay;

        private DelegateCommand changeModeListCommand;

        private DelegateCommand changeModeLoadingUnitCommand;

        private IEnumerable<TrayControlCompartment> compartments;

        private int currentItemCompartmentIndex;

        private int currentItemIndex;

        private bool isListVisibile;

        private DelegateCommand itemCompartmentDownCommand;

        private DelegateCommand itemCompartmentUpCommand;

        private DelegateCommand itemDownCommand;

        private IEnumerable<CompartmentDetails> items;

        private IEnumerable<CompartmentDetails> itemsCompartments;

        private DelegateCommand itemUpCommand;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private TrayControlCompartment selectedCompartment;

        private CompartmentDetails selectedItem;

        private CompartmentDetails selectedItemCompartment;

        private bool isBusyConfirmingRecallOperation;

        private bool isBusyConfirmingOperation;

        #endregion

        #region Constructors

        public BaseLoadingUnitViewModel(
            IBayManager bayManager,
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService,
            WMS.Data.WebAPI.Contracts.ILoadingUnitsWmsWebService loadingUnitsWmsWebService)
            : base(PresentationMode.Operator)
        {
            this.bayManager = bayManager ?? throw new ArgumentNullException(nameof(bayManager));
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService ?? throw new ArgumentNullException(nameof(machineLoadingUnitsWebService));
            this.loadingUnitsWmsWebService = loadingUnitsWmsWebService;
        }

        #endregion

        #region Properties

        public ICommand ChangeModeListCommand =>
                        this.changeModeListCommand
                        ??
                        (this.changeModeListCommand = new DelegateCommand(() => this.ChangeMode(), this.CanChangeListMode));

        public ICommand ChangeModeLoadingUnitCommand =>
               this.changeModeLoadingUnitCommand
               ??
               (this.changeModeLoadingUnitCommand = new DelegateCommand(() => this.ChangeMode(), this.CanChangeLoadingUnitMode));

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBaySideBack => this.bay?.Side is WarehouseSide.Back;

        public bool IsListVisibile
        {
            get => this.isListVisibile;
            set => this.SetProperty(ref this.isListVisibile, value, this.RaiseCanExecuteChanged);
        }

        public ICommand ItemCompartmentDownCommand =>
              this.itemCompartmentDownCommand
              ??
              (this.itemCompartmentDownCommand = new DelegateCommand(() => this.ChangeSelectedItemCompartment(false), this.ItemCompartmentCanDown));

        public ICommand ItemCompartmentUpCommand =>
              this.itemCompartmentUpCommand
              ??
              (this.itemCompartmentUpCommand = new DelegateCommand(() => this.ChangeSelectedItemCompartment(true), this.ItemCompartmentCanUp));

        public ICommand ItemDownCommand =>
                this.itemDownCommand
                ??
                (this.itemDownCommand = new DelegateCommand(() => this.ChangeSelectedItem(false), this.ItemCanDown));

        public IEnumerable<CompartmentDetails> Items
        {
            get => this.items;
            set => this.SetProperty(ref this.items, value);
        }

        public IEnumerable<CompartmentDetails> ItemsCompartments
        {
            get => this.itemsCompartments;
            set => this.SetProperty(ref this.itemsCompartments, value);
        }

        public ICommand ItemUpCommand =>
               this.itemUpCommand
               ??
               (this.itemUpCommand = new DelegateCommand(() => this.ChangeSelectedItem(true), this.ItemCanUp));

        public MAS.AutomationService.Contracts.LoadingUnit LoadingUnit { get; set; }

        public double LoadingUnitDepth
        {
            get => this.loadingUnitDepth;
            set => this.SetProperty(ref this.loadingUnitDepth, value, this.RaiseCanExecuteChanged);
        }

        public double LoadingUnitWidth
        {
            get => this.loadingUnitWidth;
            set => this.SetProperty(ref this.loadingUnitWidth, value, this.RaiseCanExecuteChanged);
        }

        public TrayControlCompartment SelectedCompartment
        {
            get => this.selectedCompartment;
            set => this.SetProperty(ref this.selectedCompartment, value, this.SetItemsAndCompartment);
        }

        public CompartmentDetails SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value, this.SetSelectedItemCompartment);
        }

        public CompartmentDetails SelectedItemCompartment
        {
            get => this.selectedItemCompartment;
            set => this.SetProperty(ref this.selectedItemCompartment, value, this.SetSelectedItemAndCompartment);
        }

        public bool IsBusyConfirmingOperation
        {
            get => this.isBusyConfirmingOperation;
            set => this.SetProperty(ref this.isBusyConfirmingOperation, value, this.RaiseCanExecuteChanged);
        }

        public bool IsBusyConfirmingRecallOperation
        {
            get => this.isBusyConfirmingRecallOperation;
            set => this.SetProperty(ref this.isBusyConfirmingRecallOperation, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public void ChangeSelectedItem(bool isUp)
        {
            if (this.items is null)
            {
                return;
            }

            if (this.items.Any())
            {
                var newIndex = isUp ? this.currentItemIndex - 1 : this.currentItemIndex + 1;

                this.currentItemIndex = Math.Max(0, Math.Min(newIndex, this.items.Count() - 1));
            }

            this.SetSelectItem();
        }

        public void ChangeSelectedItemCompartment(bool isUp)
        {
            if (this.itemsCompartments is null)
            {
                return;
            }

            if (this.itemsCompartments.Any())
            {
                var newIndex = isUp ? this.currentItemCompartmentIndex - 1 : this.currentItemCompartmentIndex + 1;

                this.currentItemCompartmentIndex = Math.Max(0, Math.Min(newIndex, this.itemsCompartments.Count() - 1));
            }

            this.SelectItemCompartment();
        }

        public async override Task OnAppearedAsync()
        {
            if (!this.isBusyConfirmingOperation
                &&
                !this.IsBusyConfirmingRecallOperation)
            {
                this.Reset();
            }

            await base.OnAppearedAsync();

            this.bay = await this.bayManager.GetBayAsync();

            if (this.Data is int loadingUnitId)
            {
                this.LoadingUnit = this.MachineService.Loadunits.SingleOrDefault(l => l.Id == loadingUnitId);
                try
                {
                    var wmsLoadingUnit = await this.loadingUnitsWmsWebService.GetByIdAsync(loadingUnitId);
                    this.LoadingUnitWidth = wmsLoadingUnit.Width;
                    this.LoadingUnitDepth = wmsLoadingUnit.Depth;

                    this.ItemsCompartments = await this.loadingUnitsWmsWebService.GetCompartmentsAsync(loadingUnitId);

                    this.Compartments = MapCompartments(this.ItemsCompartments);
                }
                catch
                {
                    // do nothing: details will not be shown
                }

                this.RaiseCanExecuteChanged();
                this.RaisePropertyChanged();
            }
        }

        public virtual void RaisePropertyChanged()
        {
            this.RaisePropertyChanged(nameof(this.SelectedCompartment));
            this.RaisePropertyChanged(nameof(this.LoadingUnit));
            this.RaisePropertyChanged(nameof(this.SelectedItem));
            this.RaisePropertyChanged(nameof(this.SelectedItemCompartment));
        }

        public async Task RecallLoadingUnitAsync()
        {
            try
            {
                this.IsWaitingForResponse = true;
                await this.machineLoadingUnitsWebService.RemoveFromBayAsync(this.LoadingUnit.Id);

                this.NavigationService.GoBack();
            }
            catch (MasWebApiException ex)
            {
                this.ShowNotification(ex);
            }
        }

        protected async override Task OnMachineModeChangedAsync(
                                                                                                                                                                                            MAS.AutomationService.Contracts.Hubs.MachineModeChangedEventArgs e)
        {
            await base.OnMachineModeChangedAsync(e);

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.changeModeLoadingUnitCommand.RaiseCanExecuteChanged();
            this.changeModeListCommand.RaiseCanExecuteChanged();
            this.itemCompartmentDownCommand.RaiseCanExecuteChanged();
            this.itemCompartmentUpCommand.RaiseCanExecuteChanged();
            this.itemDownCommand.RaiseCanExecuteChanged();
            this.itemUpCommand.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private static IEnumerable<TrayControlCompartment> MapCompartments(
                    IEnumerable<WMS.Data.WebAPI.Contracts.CompartmentDetails> compartmentsFromMission)
        {
            return compartmentsFromMission
                .GroupBy(c => c.Id)
                .Select(c => new TrayControlCompartment
                {
                    Depth = c.First().Depth,
                    Id = c.First().Id,
                    Width = c.First().Width,
                    XPosition = c.First().XPosition,
                    YPosition = c.First().YPosition,
                });
        }

        private bool CanChangeListMode()
        {
            if (this.isBusyConfirmingOperation
                ||
                this.isBusyConfirmingRecallOperation)
            {
                return false;
            }

            if (this.itemsCompartments is null)
            {
                return false;
            }

            return this.itemsCompartments.Any()                
                   &&
                   !this.isListVisibile;
        }

        private bool CanChangeLoadingUnitMode()
        {
            if (this.isBusyConfirmingOperation
                ||
                this.isBusyConfirmingRecallOperation)
            {
                return false;
            }

            if (this.itemsCompartments is null)
            {
                return false;
            }

            return this.itemsCompartments.Any()
                   &&
                   this.isListVisibile;
        }

        private void ChangeMode()
        {
            if (!this.isListVisibile
                &&
                this.selectedItemCompartment is null)
            {
                this.SelectedItemCompartment = this.itemsCompartments?.FirstOrDefault();
            }

            this.IsListVisibile = !this.IsListVisibile;
        }

        private bool ItemCanDown()
        {
            if (this.items is null)
            {
                return false;
            }

            return (this.currentItemIndex < this.items.Count() - 1);
        }

        private bool ItemCanUp()
        {
            return
                this.currentItemIndex > 0;
        }

        private bool ItemCompartmentCanDown()
        {
            if (this.itemsCompartments is null)
            {
                return false;
            }

            return
              this.currentItemCompartmentIndex < this.itemsCompartments.Count() - 1;
        }

        private bool ItemCompartmentCanUp()
        {
            return
                this.currentItemCompartmentIndex > 0;
        }

        private void Reset()
        {
            this.currentItemCompartmentIndex = 0;
            this.currentItemIndex = 0;
            this.IsListVisibile = false;
            this.SelectedItemCompartment = null;
            this.SelectedCompartment = null;
            this.SelectedItem = null;
        }

        private void SelectItemCompartment()
        {
            if (this.itemsCompartments.Any())
            {
                this.SelectedItemCompartment = this.itemsCompartments.ElementAt(this.currentItemCompartmentIndex);
            }
            else
            {
                this.SelectedItemCompartment = null;
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetItems()
        {
            this.Items = this.ItemsCompartments.Where(ic => ic.Id == this.selectedCompartment.Id);
        }

        private void SetItemsAndCompartment()
        {
            if (this.selectedCompartment is null)
            {
                this.Items = null;
                this.selectedItem = null;
                return;
            }

            this.SetItems();
            if (this.Items.Any() == true)
            {
                this.SelectedItem = this.Items.First();
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetSelectedItemAndCompartment()
        {
            if (this.selectedItemCompartment is null)
            {
                return;
            }

            this.selectedCompartment = this.Compartments.FirstOrDefault(c => c.Id == this.selectedItemCompartment.Id);
            this.SetItems();
            this.RaisePropertyChanged();
            if (this.items?.Any() == true)
            {
                if (this.items.FirstOrDefault(ic => ic.ItemId == this.selectedItemCompartment.ItemId) is CompartmentDetails newSelectedItem)
                {
                    this.currentItemIndex = this.items.ToList().IndexOf(newSelectedItem);
                    this.selectedItem = newSelectedItem;
                    this.RaisePropertyChanged();
                }
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetSelectedItemCompartment()
        {
            if (this.selectedItem is null)
            {
                this.SelectedItemCompartment = null;
                return;
            }

            if (this.itemsCompartments.FirstOrDefault(ic => ic.Id == this.selectedCompartment.Id
                                                            &&
                                                            ic.ItemId == this.selectedItem.ItemId) is CompartmentDetails newSelectedItemCompartment)
            {
                this.currentItemCompartmentIndex = this.itemsCompartments.ToList().IndexOf(newSelectedItemCompartment);
                this.selectedItemCompartment = newSelectedItemCompartment;
                this.RaisePropertyChanged();
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetSelectItem()
        {
            if (this.items?.Any() == true)
            {
                this.SelectedItem = this.items.ElementAt(this.currentItemIndex);
            }
            else
            {
                this.SelectedItem = null;
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
