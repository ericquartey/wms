using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.OperatorApp.Interfaces;
using Prism.Events;
using Prism.Mvvm;

namespace Ferretto.VW.OperatorApp.ViewsAndViewModels.Other
{
    public class ImmediateDrawerCallViewModel : BindableBase, IImmediateDrawerCallViewModel
    {
        #region Fields

        private readonly CustomControlDrawerDataGridViewModel dataGridViewModelRef;

        private readonly ObservableCollection<DataGridDrawer> drawers = new ObservableCollection<DataGridDrawer>();

        private readonly IEventAggregator eventAggregator;

        private BindableBase dataGridViewModel;

        #endregion

        #region Constructors

        public ImmediateDrawerCallViewModel(
            IEventAggregator eventAggregator,
            ICustomControlDrawerDataGridViewModel drawerDataGridViewModel)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            this.eventAggregator = eventAggregator;
            this.DrawerDataGridViewModel = drawerDataGridViewModel;
            this.dataGridViewModelRef = drawerDataGridViewModel as CustomControlDrawerDataGridViewModel;
            this.dataGridViewModelRef.Drawers = this.drawers;
            this.dataGridViewModelRef.SelectedDrawer = this.drawers.FirstOrDefault();
            this.dataGridViewModel = this.dataGridViewModelRef;

            this.NavigationViewModel = null;
            var random = new Random();
            for (var i = 0; i < random.Next(4, 20); i++)
            {
                this.drawers.Add(new DataGridDrawer
                {
                    Drawer = $"Drawer {i}",
                    Height = random.Next(100, 400).ToString(),
                    Weight = random.Next(100, 1000).ToString(),
                    Cell = random.Next(1, 200).ToString(),
                    Side = random.Next(0, 1).ToString(),
                    State = "State",
                });
            }
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public ICustomControlDrawerDataGridViewModel DrawerDataGridViewModel { get; }

        public BindableBase NavigationViewModel { get; set; }

        #endregion

        #region Methods

        public void ExitFromViewMethod()
        {
            // TODO
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
