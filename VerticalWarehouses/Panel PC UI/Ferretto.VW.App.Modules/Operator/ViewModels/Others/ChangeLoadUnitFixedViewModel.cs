using System;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Mvvm;
using Ferretto.VW.App.Resources;
using Ferretto.VW.App.Services;
using Ferretto.VW.MAS.AutomationService.Contracts;

namespace Ferretto.VW.App.Modules.Operator.ViewModels
{
    public class ChangeLoadUnitFixedViewModel : BaseOperatorViewModel
    {
        #region Fields

        private readonly IMachineLoadingUnitsWebService machineWebService;

        private bool isBusy;

        private int loadUnuitId;

        private DelegateCommand saveCommand;

        private LoadingUnit selectedUnitUnit;

        #endregion

        #region Constructors

        public ChangeLoadUnitFixedViewModel(IMachineLoadingUnitsWebService machineWebService)
                    : base(PresentationMode.Operator)
        {
            this.machineWebService = machineWebService ?? throw new ArgumentNullException(nameof(machineWebService));
        }

        #endregion

        #region Properties

        public int LoadUnitId
        {
            get => this.loadUnuitId;
            set => this.SetProperty(ref this.loadUnuitId, value, this.RaiseCanExecuteChanged);
        }

        public ICommand SaveCommand =>
                                    this.saveCommand
            ??
            (this.saveCommand = new DelegateCommand(
                async () => await this.SaveAsync(),
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

        private async Task SaveAsync()
        {
            try
            {
                this.isBusy = true;
                this.IsWaitingForResponse = true;

                await this.machineWebService.SaveLoadUnitAsync(this.SelectedLoadingUnit);

                this.ShowNotification(Localized.Get("InstallationApp.SaveSuccessful"), Services.Models.NotificationSeverity.Success);
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
