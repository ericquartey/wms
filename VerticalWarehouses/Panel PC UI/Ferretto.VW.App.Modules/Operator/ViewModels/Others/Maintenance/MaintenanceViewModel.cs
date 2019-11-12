using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services;
using Prism.Commands;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class MaintenanceViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly ICustomControlMaintenanceDataGridViewModel customControlMaintenance;

        private readonly CustomControlMaintenanceDataGridViewModel dataGridViewModelRef;

        private BindableBase dataGridViewModel;

        private bool isWaitingForResponse;

        private ObservableCollection<DataGridKit> kits;

        private ICommand maintenanceDetailButtonCommand;

        #endregion

        #region Constructors

        public MaintenanceViewModel(
            ICustomControlMaintenanceDataGridViewModel customControlMaintenance)
            : base(PresentationMode.Operator)
        {
            this.customControlMaintenance = customControlMaintenance ?? throw new ArgumentNullException(nameof(customControlMaintenance));

            this.dataGridViewModelRef = customControlMaintenance as CustomControlMaintenanceDataGridViewModel;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        public override EnableMask EnableMask => EnableMask.Any;

        public bool IsWaitingForResponse
        {
            get => this.isWaitingForResponse;
            protected set => this.SetProperty(ref this.isWaitingForResponse, value);
        }

        public ICustomControlMaintenanceDataGridViewModel MaintenanceDataGridViewModel { get; }

        public ICommand MaintenanceDetailButtonCommand =>
                    this.maintenanceDetailButtonCommand
            ??
            (this.maintenanceDetailButtonCommand = new DelegateCommand(() => this.Detail(), this.CanDetailCommand));

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.IsBackNavigationAllowed = true;

            var random = new Random();
            this.kits = new ObservableCollection<DataGridKit>();
            for (var i = 0; i < random.Next(3, 30); i++)
            {
                this.kits.Add(
                    new DataGridKit
                    {
                        Kit = $"Kit {i}",
                        Description = $"Kit number {i}",
                        State = $"State",
                        Request = "Request",
                    });
            }

            this.dataGridViewModelRef.Kits = this.kits;
            this.dataGridViewModelRef.SelectedKit = this.kits[0];
            this.DataGridViewModel = this.dataGridViewModelRef;
        }

        private bool CanDetailCommand()
        {
            return !this.IsWaitingForResponse;
        }

        private void Detail()
        {
            this.IsWaitingForResponse = true;

            try
            {
                this.NavigationService.Appear(
                    nameof(Utils.Modules.Operator),
                    Utils.Modules.Operator.Others.Maintenance.DETAIL,
                    null,
                    trackCurrentView: true);
            }
            catch (System.Exception ex)
            {
                this.ShowNotification(ex);
            }
            finally
            {
                this.IsWaitingForResponse = false;
            }
        }

        #endregion
    }
}
