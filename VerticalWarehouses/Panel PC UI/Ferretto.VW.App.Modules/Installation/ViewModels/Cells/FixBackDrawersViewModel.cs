using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.Utils.Attributes;
using Ferretto.VW.Utils.Enumerators;
using Prism.Commands;

namespace Ferretto.VW.App.Installation.ViewModels
{
    [Warning(WarningsArea.Installation)]
    internal sealed class FixBackDrawersViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly IMachineCellsWebService machineCellsWebService;

        private readonly IDialogService dialogService;

        private DelegateCommand saveCommand;

        private double stepValue;

        private bool isFree;

        #endregion

        #region Constructors

        public FixBackDrawersViewModel(
            IMachineCellsWebService machineCellsWebService,
            IDialogService dialogService)
            : base(PresentationMode.Installer)
        {
            this.machineCellsWebService = machineCellsWebService ?? throw new ArgumentNullException(nameof(machineCellsWebService));
            this.dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        }

        #endregion

        #region Properties

        public ICommand SaveCommand =>
                                    this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(), this.CanSave));

        public double StepValue
        {
            get => this.stepValue;
            set => this.SetProperty(ref this.stepValue, value, this.RaiseCanExecuteChanged);
        }
        public bool IsFree
        {
            get => this.isFree;
            set => this.SetProperty(ref this.isFree, value, this.RaiseCanExecuteChanged);
        }

        #endregion

        #region Methods

        public override void Disappear()
        {
            this.StepValue = 0.0;
            this.IsFree = true;

            base.Disappear();
        }

        public override async Task OnAppearedAsync()
        {
            this.StepValue = 0.0;
            this.IsFree = true;

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return this.StepValue != 0 && this.IsFree;
        }

        private async Task SaveAsync()
        {
            this.IsFree = false;

            try
            {
                var dialogResult = this.dialogService.ShowMessage(Localized.Get("InstallationApp.ConfirmationOperation"), Localized.Get("InstallationApp.FixBackDrawers"), DialogType.Question, DialogButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    var cells = IEnumConvert(this.MachineService.CellsPlus.Where(s => s.Side == WarehouseSide.Back).OrderBy(s => s.Id));

                    cells.ForEach(c => c.Position += this.stepValue);

                    await this.machineCellsWebService.SaveCellsAsync(cells);

                    this.NavigationService.GoBack();

                    this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"));
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsFree = true;
            }
        }

        #endregion
    }
}
