using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ChangeRotationClassViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineWebService;

        private bool isBusy;

        private int laserOffset;

        private int loadUnuitId;

        private DelegateCommand saveCommand;

        private LoadingUnit selectedUnitUnit;

        #endregion

        #region Constructors

        public ChangeRotationClassViewModel(IMachineLoadingUnitsWebService machineWebService)
                    : base(PresentationMode.Operator)
        {
            this.machineWebService = machineWebService ?? throw new ArgumentNullException(nameof(machineWebService));
        }

        #endregion

        #region Properties

        public int LaserOffset
        {
            get => this.laserOffset;
            set => this.SetProperty(ref this.laserOffset, value, this.RaiseCanExecuteChanged);
        }

        public static List<string> EnumRotationClass => new List<string>() { "A", "B", "C" };

        public int LoadUnitId
        {
            get => this.loadUnuitId;
            set => this.SetProperty(ref this.loadUnuitId, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveCommand =>
                                    this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveRotationClassAsync(),
                this.CanSave));

        public LoadingUnit SelectedLoadingUnit
        {
            get => this.selectedUnitUnit;
            set
            {
                if (this.SetProperty(ref this.selectedUnitUnit, value))
                {
                    this.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            this.LoadData();

            await base.OnAppearedAsync();
        }

        protected override void RaiseCanExecuteChanged()
        {
            base.RaiseCanExecuteChanged();

            this.saveCommand?.RaiseCanExecuteChanged();
        }

        private bool CanSave()
        {
            return !this.MachineStatus.IsMoving &&
                !this.isBusy;
        }

        private void LoadData()
        {
            try
            {
                this.isBusy = true;
                this.SelectedLoadingUnit = this.Data as LoadingUnit;
                this.LoadUnitId = this.SelectedLoadingUnit?.Id ?? 0;
                this.LaserOffset = (int)(this.SelectedLoadingUnit?.LaserOffset ?? 0);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.isBusy = false;
            }
        }

        private async Task SaveRotationClassAsync()
        {
            try
            {
                this.isBusy = true;
                this.IsWaitingForResponse = true;

                await this.machineWebService.SaveLoadUnitAsync(this.SelectedLoadingUnit);
            }
            catch (Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;

                this.isBusy = false;
            }
        }

        #endregion
    }
}
