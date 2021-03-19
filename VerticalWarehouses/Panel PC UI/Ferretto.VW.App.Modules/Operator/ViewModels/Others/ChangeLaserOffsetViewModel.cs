using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Prism.Commands;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ChangeLaserOffsetViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineWebService;

        private bool isBusy;

        private int laserOffset;

        private int loadUnuitId;

        private DelegateCommand saveLaserOffsetCommand;

        private LoadingUnit selectedUnitUnit;

        #endregion

        #region Constructors

        public ChangeLaserOffsetViewModel(IMachineLoadingUnitsWebService machineWebService)
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

        public int LoadUnitId
        {
            get => this.loadUnuitId;
            set => this.SetProperty(ref this.loadUnuitId, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveLaserOffsetCommand =>
                                            this.saveLaserOffsetCommand
            ??
            (this.saveLaserOffsetCommand = new DelegateCommand(
                async () => await this.SaveLaserOffsetAsync(),
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

            this.saveLaserOffsetCommand?.RaiseCanExecuteChanged();
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

        private async Task SaveLaserOffsetAsync()
        {
            try
            {
                this.isBusy = true;
                this.IsWaitingForResponse = true;

                await this.machineWebService.SetLoadingUnitOffsetAsync(this.LoadUnitId, this.LaserOffset);
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
