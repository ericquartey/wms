using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.VW.App.Controls;
using Ferretto.VW.App.Controls.Controls;
using Ferretto.VW.App.Controls.Interfaces;
using Ferretto.VW.App.Controls.Utils;
using Ferretto.VW.App.Services;
using Prism.Mvvm;

namespace Ferretto.VW.App.Operator.ViewModels
{
    public class ImmediateDrawerCallViewModel : BaseMainViewModel
    {
        #region Fields

        private readonly ICustomControlDrawerDataGridViewModel drawerDataGridViewModel;

        private readonly ObservableCollection<DataGridDrawer> drawers = new ObservableCollection<DataGridDrawer>();

        private BindableBase dataGridViewModel;

        #endregion

        #region Constructors

        public ImmediateDrawerCallViewModel(ICustomControlDrawerDataGridViewModel drawerDataGridViewModel)
            : base(PresentationMode.Operator)
        {
            this.drawerDataGridViewModel = drawerDataGridViewModel ?? throw new ArgumentNullException(nameof(drawerDataGridViewModel));
        }

        #endregion

        #region Properties

        public BindableBase DataGridViewModel { get => this.dataGridViewModel; set => this.SetProperty(ref this.dataGridViewModel, value); }

        public override EnableMask EnableMask => EnableMask.Any;

        #endregion

        #region Methods

        public override async Task OnAppearedAsync()
        {
            await base.OnAppearedAsync();

            this.drawers.Clear();

            var random = new Random();
            for (var i = 0; i < random.Next(4, 30); i++)
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

            var dataGridViewModelRef = this.drawerDataGridViewModel as CustomControlDrawerDataGridViewModel;

            dataGridViewModelRef.Drawers = this.drawers;
            dataGridViewModelRef.SelectedDrawer = this.drawers.FirstOrDefault();
            this.dataGridViewModel = dataGridViewModelRef;

            this.RaisePropertyChanged(nameof(this.DataGridViewModel));

            this.IsBackNavigationAllowed = true;
        }

        #endregion
    }
}
