using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.CustomControls.Controls;
using Ferretto.VW.OperatorApp.Interfaces;
using Unity;
using Prism.Events;
using Prism.Mvvm;
using Ferretto.VW.WmsCommunication.Interfaces;
using Ferretto.VW.CustomControls.Interfaces;
using System.Collections.ObjectModel;
using Ferretto.VW.CustomControls.Utils;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class ImmediateDrawerCallViewModel : BindableBase, IImmediateDrawerCallViewModel
    {
        #region Fields

        private IUnityContainer container;

        private BindableBase dataGridViewModel;

        private CustomControlDrawerDataGridViewModel dataGridViewModelRef;

        private ObservableCollection<DataGridDrawer> drawers = new ObservableCollection<DataGridDrawer>();

        private IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public ImmediateDrawerCallViewModel(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.NavigationViewModel = null;
            var random = new Random();
            for (int i = 0; i < random.Next(4, 20); i++)
            {
                this.drawers.Add(new DataGridDrawer
                {
                    Drawer = $"Drawer {i}",
                    Height = random.Next(100, 400).ToString(),
                    Weight = random.Next(100, 1000).ToString(),
                    Cell = random.Next(1, 200).ToString(),
                    Side = random.Next(0, 1).ToString(),
                    State = "State",
                }
                );
            }
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
            this.dataGridViewModelRef = this.container.Resolve<ICustomControlDrawerDataGridViewModel>() as CustomControlDrawerDataGridViewModel;
            this.dataGridViewModelRef.Drawers = this.drawers;
            this.dataGridViewModelRef.SelectedDrawer = this.drawers[0];
            this.dataGridViewModel = this.dataGridViewModelRef;
        }

        public async Task OnEnterViewAsync()
        {
            // TODO
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
