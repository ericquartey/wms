using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class LoadingUnitEditViewModel : BaseServiceNavigationViewModel, IRefreshDataEntityViewModel
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider = ServiceLocator.Current.GetInstance<ICompartmentProvider>();
        private readonly ILoadingUnitProvider loadingUnitProvider = ServiceLocator.Current.GetInstance<ILoadingUnitProvider>();
        private ICommand addCommand;
        private ICommand bulkAddCommand;
        private IEnumerable<CompartmentDetails> compartmentsDataSource;
        private ICommand editCommand;
        private Func<CompartmentDetails, CompartmentDetails, string> filterColorFunc;
        private InputAddCompartmentViewModel inputAddVM;
        private InputBulkAddCompartmentViewModel inputBulkAddVM;
        private InputEditCompartmentViewModel inputEditVM;
        private bool isEnabledGrid;
        private bool isExpand;
        private bool isReadOnlyTray;
        private bool isSelectableTray;
        private bool isVisibleMainCommandBar;
        private LoadingUnitDetails loadingUnit;
        private bool loadingUnitHasCompartments;
        private bool readOnlyTray;
        private CompartmentDetails selectedCompartmentTray;
        private Tray tray;

        #endregion Fields

        #region Constructors

        public LoadingUnitEditViewModel()
        {
            this.ShowMainViewAndHideLateralPanel();
        }

        #endregion Constructors

        #region Properties

        public ICommand AddCommand => this.addCommand ??
                  (this.addCommand = new DelegateCommand(this.ExecuteAddCompartmentCommand));

        public ICommand BulkAddCommand => this.bulkAddCommand ??
                                  (this.bulkAddCommand = new DelegateCommand(this.ExecuteBulkAddCommand));

        public IEnumerable<CompartmentDetails> CompartmentsDataSource
        {
            get => this.compartmentsDataSource;
            set => this.SetProperty(ref this.compartmentsDataSource, value);
        }

        public ICommand EditCommand => this.editCommand ??
          (this.editCommand = new DelegateCommand(this.ExecuteEditCompartmentCommand, this.CanExecuteEditCommand)
            .ObservesProperty(() => this.SelectedCompartmentTray));

        public Func<CompartmentDetails, CompartmentDetails, string> FilterColorFunc
        {
            get => this.filterColorFunc;
            set => this.SetProperty(ref this.filterColorFunc, value);
        }

        public InputAddCompartmentViewModel InputAddVM
        {
            get => this.inputAddVM;
            set => this.SetProperty(ref this.inputAddVM, value);
        }

        public InputBulkAddCompartmentViewModel InputBulkAddVM
        {
            get => this.inputBulkAddVM;
            set => this.SetProperty(ref this.inputBulkAddVM, value);
        }

        public InputEditCompartmentViewModel InputEditVM
        {
            get => this.inputEditVM;
            set => this.SetProperty(ref this.inputEditVM, value);
        }

        public bool IsEnabledGrid
        {
            get => this.isEnabledGrid;
            set => this.SetProperty(ref this.isEnabledGrid, value);
        }

        public bool IsExpand
        {
            get => this.isExpand;
            set => this.SetProperty(ref this.isExpand, value);
        }

        public bool IsReadOnlyTray
        {
            get => this.isReadOnlyTray;
            set => this.SetProperty(ref this.isReadOnlyTray, value);
        }

        public bool IsSelectableTray
        {
            get => this.isSelectableTray;
            set => this.SetProperty(ref this.isSelectableTray, value);
        }

        public bool IsVisibleMainCommandBar
        {
            get => this.isVisibleMainCommandBar;
            set => this.SetProperty(ref this.isVisibleMainCommandBar, value);
        }

        public LoadingUnitDetails LoadingUnit
        {
            get => this.loadingUnit;
            set
            {
                if (!this.SetProperty(ref this.loadingUnit, value))
                {
                    return;
                }
                this.RefreshData();
            }
        }

        public bool LoadingUnitHasCompartments
        {
            get => this.loadingUnitHasCompartments;
            set => this.SetProperty(ref this.loadingUnitHasCompartments, value);
        }

        public bool ReadOnlyTray
        {
            get => this.readOnlyTray;
            set => this.SetProperty(ref this.readOnlyTray, value);
        }

        public CompartmentDetails SelectedCompartmentTray
        {
            get => this.selectedCompartmentTray;
            set
            {
                if (value != null)
                {
                    this.SetProperty(ref this.selectedCompartmentTray, value);
                }
            }
        }

        public Tray Tray
        {
            get => this.tray;
            set => this.SetProperty(ref this.tray, value);
        }

        #endregion Properties

        #region Methods

        public void RefreshData()
        {
            this.CompartmentsDataSource = this.loadingUnit != null
                ? this.compartmentProvider.GetByLoadingUnitId(this.loadingUnit.Id).ToList()
                : null;
        }

        protected override void OnAppear()
        {
            this.LoadData();
            base.OnAppear();
        }

        private bool CanExecuteEditCommand()
        {
            return (this.selectedCompartmentTray != null && this.selectedCompartmentTray.Width != null && this.selectedCompartmentTray.Height != null
                && this.selectedCompartmentTray.XPosition != null && this.selectedCompartmentTray.YPosition != null);
        }

        private void ExecuteAddCompartmentCommand()
        {
            this.SelectedCompartmentTray = new CompartmentDetails();
            this.HideMainViewAndShowLateralPanel();
            this.InputAddVM.Initialize(this.tray, this.loadingUnit.Id);
            this.InputAddVM.FinishEvent += this.InputAddVM_FinishEvent;
        }

        private void ExecuteBulkAddCommand()
        {
            this.SelectedCompartmentTray = null;
            this.HideMainViewAndShowLateralPanel();
            this.InputBulkAddVM.Initialize(this.tray);
            this.InputBulkAddVM.FinishEvent += this.InputBulkAddVM_FinishEvent;
        }

        private void ExecuteEditCompartmentCommand()
        {
            this.HideMainViewAndShowLateralPanel();
            this.PopulatePairing();
            this.InputEditVM.Initialize(this.tray, this.loadingUnit, this.selectedCompartmentTray);
            this.InputEditVM.FinishEvent += this.InputEditVM_FinishEvent;
        }

        private void HideMainViewAndShowLateralPanel()
        {
            this.IsExpand = true;
            this.IsSelectableTray = false;
            this.ReadOnlyTray = true;
            this.IsEnabledGrid = false;
            this.IsVisibleMainCommandBar = false;
            this.FilterColorFunc = (new EditReadOnlyFilter()).ColorFunc;
        }

        private void InitializeTray()
        {
            this.tray = new Tray
            {
                Dimension = new Dimension
                {
                    Height = this.LoadingUnit.Length,
                    Width = this.LoadingUnit.Width
                },
                LoadingUnitId = this.LoadingUnit.Id,
            };
            if (this.LoadingUnit.Compartments != null)
            {
                this.tray.AddCompartmentsRange(this.LoadingUnit.Compartments);
            }

            this.FilterColorFunc = (new EditFilter()).ColorFunc;

            this.RaisePropertyChanged(nameof(this.Tray));
        }

        private void InputAddVM_FinishEvent(Object sender, EventArgs e)
        {
            this.ResetInputView();
            if (sender is InputAddCompartmentViewModel model)
            {
                this.SelectedCompartmentTray = model.Compartment;
            }
            this.InputAddVM.FinishEvent -= this.InputAddVM_FinishEvent;
        }

        private void InputBulkAddVM_FinishEvent(Object sender, EventArgs e)
        {
            this.ResetInputView();
            this.InputBulkAddVM.FinishEvent -= this.InputBulkAddVM_FinishEvent;
        }

        private void InputEditVM_FinishEvent(Object sender, EventArgs e)
        {
            this.ResetInputView();
            this.InputEditVM.FinishEvent -= this.InputEditVM_FinishEvent;
        }

        private void LoadData()
        {
            if (this.Data is int modelId)
            {
                this.LoadingUnit = this.loadingUnitProvider.GetById(modelId);
                this.InitializeTray();
            }
        }

        private void PopulatePairing()
        {
            this.selectedCompartmentTray.ItemPairingChoices = this.loadingUnit.CellPairingChoices;
        }

        private void ResetInputView()
        {
            this.ShowMainViewAndHideLateralPanel();
        }

        private void ShowMainViewAndHideLateralPanel()
        {
            this.IsExpand = false;
            this.IsSelectableTray = true;
            this.ReadOnlyTray = false;
            this.IsEnabledGrid = true;
            this.IsVisibleMainCommandBar = true;
            this.FilterColorFunc = (new EditFilter()).ColorFunc;
        }

        #endregion Methods
    }
}
