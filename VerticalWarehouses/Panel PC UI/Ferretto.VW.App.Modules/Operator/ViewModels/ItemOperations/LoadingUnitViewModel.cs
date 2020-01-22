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

        private IEnumerable<TrayControlCompartment> compartments;

        private DelegateCommand confirmOperationCommand;

        private int currentItemIndex;

        private DelegateCommand downCommand;

        private DelegateCommand changeModeCommand;

        private bool isListVisibile;

        private IEnumerable<CompartmentDetails> items;

        private IEnumerable<CompartmentDetails> itemsCompartments;

        private double loadingUnitDepth;

        private double loadingUnitWidth;

        private CompartmentDetails selectedItem;

        private CompartmentDetails selectedItemCompartment;

        private DelegateCommand upCommand;

        private DelegateCommand itemCompartmentDownCommand;

        private DelegateCommand itemCompartmentUpCommand;
        private int currentItemCompartmentIndex;

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

        public IEnumerable<TrayControlCompartment> Compartments
        {
            get => this.compartments;
            set => this.SetProperty(ref this.compartments, value);
        }

        public ICommand ConfirmOperationCommand =>
            this.confirmOperationCommand
            ??
            (this.confirmOperationCommand = new DelegateCommand(
                async () => await this.ConfirmOperationAsync(),
                this.CanConfirmOperation));
        
        public ICommand DownCommand =>
                this.downCommand
                ??
                (this.downCommand = new DelegateCommand(() => this.ChangeSelectedItem(true), this.CanDown));

        public ICommand ChangeModeCommand =>
                this.changeModeCommand
                ??
                (this.changeModeCommand = new DelegateCommand(() => this.ChangeMode(), this.CanDown));

        private void ChangeMode()
        {
            this.IsListVisibile = !this.IsListVisibile;           
        }

        public override EnableMask EnableMask => EnableMask.Any;
         
        public bool IsBaySideBack => this.bay?.Side is WarehouseSide.Back;

        public bool IsListVisibile
        {
            get => this.isListVisibile;
            set => this.SetProperty(ref this.isListVisibile, value, this.RaiseCanExecuteChanged);
        }

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

        public CompartmentDetails SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value);
        }

        public CompartmentDetails SelectedItemCompartment
        {
            get => this.selectedItemCompartment;
            set => this.SetProperty(ref this.selectedItemCompartment, value);
        }
        

        public ICommand UpCommand =>
               this.upCommand
               ??
               (this.upCommand = new DelegateCommand(() => this.ChangeSelectedItem(true), this.CanUp));

        public ICommand ItemCompartmentUpCommand =>
              this.itemCompartmentUpCommand
              ??
              (this.itemCompartmentUpCommand = new DelegateCommand(() => this.ChangeSelectedCompartment(true), this.CanUp));

        public ICommand ItemCompartmentDownCommand =>
              this.itemCompartmentDownCommand
              ??
              (this.itemCompartmentDownCommand = new DelegateCommand(() => this.ChangeSelectedCompartment(true), this.CanUp));
        
        #endregion

        #region Methods

        public virtual bool CanConfirmOperation()
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

            this.SelectItem();
        }

        public void ChangeSelectedCompartment(bool isUp)
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

            this.SelectCompartment();
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
            await base.OnAppearedAsync();

            this.bay = await this.bayManager.GetBayAsync();

            if (this.Data is int loadingUnitId)
            {
                this.LoadingUnit = this.MachineService.Loadunits.SingleOrDefault(l => l.Id == loadingUnitId);
                try
                {
                    this.SelectedItem = null;

                    var wmsLoadingUnit = await this.loadingUnitsWmsWebService.GetByIdAsync(loadingUnitId);
                    this.LoadingUnitWidth = wmsLoadingUnit.Width;
                    this.LoadingUnitDepth = wmsLoadingUnit.Depth;

                    this.Items = await this.loadingUnitsWmsWebService.GetCompartmentsAsync(loadingUnitId);
                    this.ItemsCompartments = this.Items;
                    this.Compartments = MapCompartments(this.items);
                }
                catch
                {
                    // do nothing: details will not be shown
                }

                this.RaisePropertyChanged(nameof(this.LoadingUnit));
                this.confirmOperationCommand?.RaiseCanExecuteChanged();
            }
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

        private bool CanDown()
        {
            if (this.items is null)
            {
                return false;
            }

            return
              this.currentItemIndex < this.items.Count() - 1;
        }

        private bool CanUp()
        {
            return
                this.currentItemIndex > 0;
        }

        private void SelectItem()
        {
            if (this.items.Any())
            {
                this.SelectedItem = this.items.ElementAt(this.currentItemIndex);
            }
            else
            {
                this.SelectedItem = null;
            }

            this.RaiseCanExecuteChanged();
        }

        private void SelectCompartment()
        {
            if (this.itemsCompartments.Any())
            {
                this.SelectedItemCompartment = this.items.ElementAt(this.currentItemCompartmentIndex);
            }
            else
            {
                this.SelectedItemCompartment = null;
            }

            this.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
