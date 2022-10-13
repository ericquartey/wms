using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class CellsViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IMachineIdentityWebService machineIdentityWebService;

        private bool anyCellSelected;

        private BlockLevel cellsBlockLevels;

        private SubscriptionToken cellsToken;

        private string description;

        private bool isBusy;

        private bool isRotationClassEnabled;

        private DelegateCommand saveCommand;

        private CellPlus selectedCell;

        private List<CellPlus> selectedCells = new List<CellPlus>();

        #endregion

        #region Constructors

        public CellsViewModel(
            IMachineCellsWebService machineCellsWebService,
            IMachineIdentityWebService machineIdentityWebService)
            : base(PresentationMode.Installer)
        {
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.machineIdentityWebService = machineIdentityWebService ?? throw new ArgumentNullException(nameof(machineIdentityWebService));
        }

        #endregion

        #region Properties

        public bool AnyCellSelected
        {
            get => this.anyCellSelected;
            set => this.SetProperty(ref this.anyCellSelected, value);
        }

        public IEnumerable<BlockLevel> BlockLevels => Enum.GetValues(typeof(BlockLevel)).OfType<BlockLevel>().Where(block => block != BlockLevel.NeedsTest && block != BlockLevel.Undefined).ToList();

        public ObservableCollection<CellPlus> Cells => IEnumConvert(this.MachineService.CellsPlus.OrderBy(s => s.Side).ThenBy(s => s.Id));

        public BlockLevel CellsBlockLevels
        {
            get => this.cellsBlockLevels;
            set => this.SetProperty(ref this.cellsBlockLevels, value);
        }

        public string Description
        {
            get => this.description;
            set => this.SetProperty(ref this.description, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsBusy
        {
            get => this.isBusy;
            set => this.SetProperty(ref this.isBusy, value);
        }

        public bool IsEnabledEditing => !this.IsMoving;

        public bool IsRotationClassEnabled
        {
            get => this.isRotationClassEnabled;
            set => this.SetProperty(ref this.isRotationClassEnabled, value);
        }

        public ICommand SaveCommand =>
            this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.Save(),
                () => !this.IsMoving && this.SelectedCells != null && this.SelectedCells.Any()));

        public CellPlus SelectedCell
        {
            get => this.selectedCell;
            set => this.SetProperty(ref this.selectedCell, value);
        }

        public List<CellPlus> SelectedCells
        {
            get => this.selectedCells;
            set
            {
                this.SetProperty(ref this.selectedCells, value);

                if (this.selectedCells != null && !this.selectedCells.Any())
                {
                    this.SelectedCell = null;
                    this.AnyCellSelected = false;
                }
                else if (this.selectedCells != null && this.selectedCells.Count == 1)
                {
                    this.SelectedCell = this.selectedCells.Single();
                    this.AnyCellSelected = true;
                }
                else if (this.selectedCells != null && this.selectedCells.Count > 1)
                {
                    this.SelectedCell = null;
                    this.CellsBlockLevels = this.selectedCells.First().BlockLevel;
                    this.Description = string.Empty;
                    this.AnyCellSelected = true;
                }
            }
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            base.Disappear();

            if (this.cellsToken != null)
            {
                this.EventAggregator.GetEvent<CellsChangedPubSubEvent>().Unsubscribe(this.cellsToken);
                this.cellsToken.Dispose();
                this.cellsToken = null;
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.IsRotationClassEnabled = await this.machineIdentityWebService.GetIsRotationClassAsync();

            this.SubscribeToEvents();

            this.RaisePropertyChanged(nameof(this.Cells));

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();

            this.RaisePropertyChanged(nameof(this.BlockLevels));
            this.RaisePropertyChanged(nameof(this.IsEnabledEditing));
            this.RaisePropertyChanged(nameof(this.IsBusy));
            this.RaisePropertyChanged(nameof(this.Cells));

            if (this.IsMoving)
            {
                this.ShowNotification(Localized.Get("InstallationApp.MovingMachine"), Services.Models.NotificationSeverity.Error);
            }
        }

        private async Task Save()
        {
            try
            {
                this.IsBusy = true;

                this.ClearNotifications();

                if (this.selectedCells.Count == 1)
                {
                    await this.machineCellsWebService.SaveCellAsync(this.SelectedCell);
                }
                else
                {
                    foreach (var cell in this.selectedCells)
                    {
                        cell.BlockLevel = this.cellsBlockLevels;
                        cell.Description = this.description;
                        // multiselection do not change free state
                        var backup = this.MachineService.Cells.FirstOrDefault(c => c.Id == cell.Id);
                        if (backup != null)
                        {
                            cell.IsFree = backup.IsFree;
                        }
                        await this.machineCellsWebService.SaveCellAsync(cell);
                    }
                }

                this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Success);
            }
            catch (Exception ex) when (ex is MasWebApiException || ex is System.Net.Http.HttpRequestException)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                await this.MachineService.GetCells();

                this.IsBusy = false;

                this.RaisePropertyChanged(nameof(this.Cells));
            }
        }

        private void SubscribeToEvents()
        {
            this.cellsToken = this.cellsToken
                 ??
                 this.EventAggregator
                     .GetEvent<CellsChangedPubSubEvent>()
                     .Subscribe(
                         m => this.RaiseCanExecuteChanged(),
                         ThreadOption.UIThread,
                         false);
        }

        #endregion
    }
}
