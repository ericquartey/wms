using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CellsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IHealthProbeService healthProbeService;

        private readonly IMachineCellsWebService machineCellsWebService;

        private int currentIndex;

        private DelegateCommand downDataGridButtonCommand;

        private Cell selectedCell;

        private DelegateCommand upDataGridButtonCommand;

        #endregion

        #region Constructors

        public CellsViewModel(
            IMachineCellsWebService machineCellsWebService,
            IHealthProbeService healthProbeService)
            : base(Services.PresentationMode.Installer)
        {
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.healthProbeService = healthProbeService ?? throw new ArgumentNullException(nameof(healthProbeService));
        }

        #endregion

        #region Properties

        public IEnumerable<Cell> Cells => this.MachineService.Cells;

        public ICommand DownDataGridButtonCommand =>
            this.downDataGridButtonCommand
            ??
            (this.downDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(false), this.IsWaitingForResponse));

        public override EnableMask EnableMask => EnableMask.Any;

        public Cell SelectedCell
        {
            get => this.selectedCell;
            set => this.SetProperty(ref this.selectedCell, value);
        }

        public ICommand UpDataGridButtonCommand =>
            this.upDataGridButtonCommand
            ??
            (this.upDataGridButtonCommand = new DelegateCommand(() => this.ChangeSelectedItemAsync(true), this.IsWaitingForResponse));

        #endregion

        #region Methods

        public void ChangeSelectedItemAsync(bool isUp)
        {
            if (this.Cells == null)
            {
                return;
            }

            if (this.Cells.Count() > 0)
            {
                this.currentIndex = isUp ? --this.currentIndex : ++this.currentIndex;
                if (this.currentIndex < 0 || this.currentIndex >= this.Cells.Count())
                {
                    this.currentIndex = (this.currentIndex < 0) ? 0 : this.Cells.Count() - 1;
                }

                this.SelectedCell = this.Cells?.ToList()[this.currentIndex];
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsBackNavigationAllowed = true;

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.Cells));
        }

        #endregion
    }
}
