using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Resources;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;
using Prism.Events;

namespace Ferretto.VW.App.Installation.ViewModels
{
    internal sealed class ElevatorWeightCheckStep1ViewModel : BaseElevatorWeightCheckViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineLoadingUnitsWebService;

        private DelegateCommand checkLoadingUnitCommand;

        private int? inputLoadingUnitId;

        private bool isloadingUnitVerified;

        private IEnumerable<LoadingUnit> loadingUnits;

        #endregion

        #region Constructors

        public ElevatorWeightCheckStep1ViewModel(
            IMachineLoadingUnitsWebService machineLoadingUnitsWebService)
            : base()
        {
            this.machineLoadingUnitsWebService = machineLoadingUnitsWebService;
        }

        #endregion

        #region Properties

        public ICommand CheckLoadingUnitCommand =>
            this.checkLoadingUnitCommand
            ??
            (this.checkLoadingUnitCommand = new DelegateCommand(
                this.CheckLoadingUnit,
                this.CanCheckLoadingUnit));

        public string Error => string.Join(
                      Environment.NewLine,
                      this[nameof(this.InputLoadingUnitId)]);

        public int? InputLoadingUnitId
        {
            get => this.inputLoadingUnitId;
            set
            {
                if (this.SetProperty(ref this.inputLoadingUnitId, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Indexers

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.InputLoadingUnitId):
                        if (!this.InputLoadingUnitId.HasValue)
                        {
                            return Localized.Get("InstallationApp.LoadingUnitIdRequired");
                        }

                        if (this.InputLoadingUnitId.Value <= 0)
                        {
                            return Localized.Get("InstallationApp.LoadingUnitIdMustBePositive");
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public async Task GetLoadingUnitsAsync()
        {
            try
            {
                this.loadingUnits = await this.machineLoadingUnitsWebService.GetAllAsync();
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        public override async Task OnAppearedAsync()
        {
            this.InputLoadingUnitId = null;
            this.isloadingUnitVerified = false;

            await base.OnAppearedAsync();

            await this.GetLoadingUnitsAsync();

            this.RaiseCanExecuteChanged();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.checkLoadingUnitCommand?.RaiseCanExecuteChanged();
        }

        private bool CanCheckLoadingUnit()
        {
            return !(this.loadingUnits is null)
              &&
              this.loadingUnits.Count() > 0
              &&
              this.InputLoadingUnitId.HasValue
              &&
              string.IsNullOrWhiteSpace(this.Error);
        }

        private void CheckLoadingUnit()
        {
            try
            {
                if (this.loadingUnits.SingleOrDefault(l => l.Id == this.inputLoadingUnitId.Value) is LoadingUnit loadingUnitFound)
                {
                    this.isloadingUnitVerified = true;
                    this.NavigateToNextStep();
                }
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
        }

        private void NavigateToNextStep()
        {
            if (this.NavigationService.IsActiveView(nameof(Utils.Modules.Installation), Utils.Modules.Installation.Elevator.WeightCheck.STEP1))
            {
                this.NavigationService.Appear(
                nameof(Utils.Modules.Installation),
                Utils.Modules.Installation.Elevator.WeightCheck.STEP2,
                this.inputLoadingUnitId.Value,
                trackCurrentView: false);
            }
        }

        private void ShowSteps()
        {
            this.ShowPrevStep(true, false);
            this.ShowNextStep(true, this.isloadingUnitVerified, nameof(Utils.Modules.Installation), Utils.Modules.Installation.Elevator.WeightCheck.STEP2);
            this.ShowAbortStep(true, true);
        }

        #endregion
    }
}
