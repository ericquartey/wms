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
    public class LoadingUnitViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IBayManager bayManager;

        private readonly WMS.Data.WebAPI.Contracts.ILoadingUnitsWmsWebService loadingUnitsWmsWebService;

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

        private DelegateCommand<string> operationCommand;

        private DelegateCommand recallLoadingUnitCommand;

        private TrayControlCompartment selectedCompartment;

        private CompartmentDetails selectedItem;

        private CompartmentDetails selectedItemCompartment;

        #endregion

        #region Constructors

        public LoadingUnitViewModel(
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

        public ICommand OperationCommand =>
               this.operationCommand
                ??
                (this.operationCommand = new DelegateCommand<string>((param) => this.DoOperation(param), this.CanDoOperation));

        public ICommand RecallLoadingUnitCommand =>
            this.recallLoadingUnitCommand
            ??
            (this.recallLoadingUnitCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanRecallLoadingUnit));

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

        #endregion

        #region Methods

        public virtual bool CanRecallLoadingUnit()
        {
            return
                !this.IsWaitingForResponse
                &&
                this.LoadingUnit != null
                &&
                this.MachineModeService.MachineMode is MachineMode.Automatic;
        }

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

        public async Task ConfirmOperationAsync()
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

        public async override Task OnAppearedAsync()
        {
            this.Reset();

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
                this.RaisePropertyChanged(nameof(this.LoadingUnit));
                this.recallLoadingUnitCommand?.RaiseCanExecuteChanged();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            this.changeModeLoadingUnitCommand.RaiseCanExecuteChanged();
            this.changeModeListCommand.RaiseCanExecuteChanged();
            this.itemCompartmentDownCommand.RaiseCanExecuteChanged();
            this.itemCompartmentUpCommand.RaiseCanExecuteChanged();
            this.itemDownCommand.RaiseCanExecuteChanged();
            this.itemUpCommand.RaiseCanExecuteChanged();
            this.operationCommand.RaiseCanExecuteChanged();

            base.RaiseCanExecuteChanged();
        }

        private static IEnumerable<TrayControlCompartment> MapCompartments(
                    IEnumerable<WMS.Data.WebAPI.Contracts.CompartmentDetails> compartmentsFromMission)
        {
            return compartmentsFromMission
                .GroupBy(c => c.Id)
                .First()
                .Select(c => new TrayControlCompartment
                {
                    Depth = c.Depth,
                    Id = c.Id,
                    Width = c.Width,
                    XPosition = c.XPosition,
                    YPosition = c.YPosition,
                });
        }

        private bool CanChangeListMode()
        {
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
            if (this.itemsCompartments is null)
            {
                return false;
            }

            return this.itemsCompartments.Any()
                   &&
                   this.isListVisibile;
        }

        private bool CanDoOperation(string param)
        {
            return !(this.selectedItem is null)
                &&
                this.MachineModeService.MachineMode is MachineMode.Automatic;
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

        private void DoOperation(string param)
        {
            var dataSend = (param, this.selectedItemCompartment);
            // TODO implement specific operation
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

        private void SetItemsAndCompartment()
        {
            if (this.selectedCompartment is null)
            {
                this.Items = null;
                this.selectedItem = null;
                return;
            }

            this.Items = this.ItemsCompartments.Where(ic => ic.Id == this.selectedCompartment.Id);
            if (!(this.Items is null)
                &&
                this.Items.Count() == 1)
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

            this.SelectedCompartment = this.Compartments.FirstOrDefault(c => c.Id == this.selectedItemCompartment.Id);
            if (!(this.Items is null)
                &&
                this.items.Any())
            {
                if (this.items.FirstOrDefault(ic => ic.ItemId == this.selectedItemCompartment.ItemId) is CompartmentDetails newSelectedItem)
                {
                    this.SelectedItem = newSelectedItem;
                    this.currentItemIndex = this.items.ToList().IndexOf(newSelectedItem);
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
                this.SelectedItemCompartment = newSelectedItemCompartment;
                this.currentItemCompartmentIndex = this.itemsCompartments.ToList().IndexOf(newSelectedItemCompartment);
            }

            this.RaiseCanExecuteChanged();
        }

        private void SetSelectItem()
        {
            if (!(this.items is null)
                &&
                this.items.Any())
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
