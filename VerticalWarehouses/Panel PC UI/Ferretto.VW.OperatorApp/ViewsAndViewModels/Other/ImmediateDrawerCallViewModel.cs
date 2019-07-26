using System;
using System.Collections.ObjectModel;
using System.Linq;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Operator.Interfaces;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewsAndViewModels.Other
{
    public class ImmediateDrawerCallViewModel : BaseViewModel, IImmediateDrawerCallViewModel
    {
        #region Fields

        private readonly CustomControlDrawerDataGridViewModel dataGridViewModelRef;

        private readonly ObservableCollection<DataGridDrawer> drawers = new ObservableCollection<DataGridDrawer>();

        private BindableBase dataGridViewModel;

        #endregion

        #region Constructors

        public ImmediateDrawerCallViewModel(
            ICustomControlDrawerDataGridViewModel drawerDataGridViewModel)
        {
            if (drawerDataGridViewModel == null)
            {
                throw new ArgumentNullException(nameof(drawerDataGridViewModel));
            }

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

        #endregion
    }
}
