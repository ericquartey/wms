using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;
using Unity;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class MaintenanceDetailViewModel : BindableBase, IMaintenanceDetailViewModel
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlMaintenanceDetailDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<DataGridMaintenanceDetail> maintenanceDetails;

        #endregion

        #region Constructors

        public MaintenanceDetailViewModel(
            IEventAggregator eventAggregator,
            ICustomControlMaintenanceDetailDataGridViewModel maintenanceDataGridViewModel)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.MaintenanceDataGridViewModel = maintenanceDataGridViewModel;
            this.dataGridViewModelRef = maintenanceDataGridViewModel as CustomControlMaintenanceDetailDataGridViewModel;

            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel
        {
            get => this.dataGridViewModel;
            set => this.SetProperty(ref this.dataGridViewModel, value);
        }

        public ICustomControlMaintenanceDetailDataGridViewModel MaintenanceDataGridViewModel { get; }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
        }

        public void InitializeViewModel(IUnityContainer container)
        {
            this.container = container;
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlMaintenanceDetailDataGridViewModel>() as CustomControlMaintenanceDetailDataGridViewModel;
        }

        public async Task OnEnterViewAsync()
        {
            var random = new Random();
            this.maintenanceDetails = new ObservableCollection<DataGridMaintenanceDetail>();
            for (int i = 0; i < random.Next(3, 30); i++)
            {
                this.maintenanceDetails.Add(new DataGridMaintenanceDetail
                {
                    Element = $"Element {i}",
                    Description = $"This is element {i}",
                    Quantity = random.Next(2, 40).ToString(),
                }
                );
            }
            this.dataGridViewModelRef.MaintenanceDetails = this.maintenanceDetails;
            this.dataGridViewModelRef.SelectedMaintenanceDetail = this.maintenanceDetails[0];
            this.DataGridViewModel = this.dataGridViewModelRef;
        }

        public void SubscribeMethodToEvent()
        {
            // TODO
        }

        public void UnSubscribeMethodFromEvent()
        {
            // TODO
        }

        #endregion
    }
}
