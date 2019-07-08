using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.CustomControls.Interfaces;
using Ferretto.VW.CustomControls.Utils;
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

        public MaintenanceDetailViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

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
