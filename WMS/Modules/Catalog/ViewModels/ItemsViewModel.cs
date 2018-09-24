using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.Catalog
{
    public class ItemsViewModel : BaseNavigationViewModel
    {
        #region Fields

        private readonly IDataSourceService filterService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private IDataSource<Common.Models.Item> selectedDataSource;
        private Tile selectedFilter;
        private ICommand viewDetailsCommand;

        #endregion Fields

        #region Properties

        public IEnumerable<IDataSource<Common.Models.Item>> DataSources => this.filterService.GetAll(MvvmNaming.GetViewNameFromViewModelName(nameof(ItemsViewModel))) as IEnumerable<IDataSource<Common.Models.Item>>;

        public IEnumerable<Tile> Filters => this.DataSources.Select(dataSource =>
            new Tile
            {
                Name = dataSource.Name,
                Count = dataSource.Count
            }
        );

        public IDataSource<Common.Models.Item> SelectedDataSource
        {
            get => this.selectedDataSource;
            set => this.SetProperty(ref this.selectedDataSource, value);
        }

        public Tile SelectedFilter
        {
            get => this.selectedFilter;
            set
            {
                if (this.SetProperty(ref this.selectedFilter, value))
                {
                    this.SelectedDataSource = this.DataSources.First(dataSource => dataSource.Name == value.Name);
                }
            }
        }

        public ICommand ViewDetailsCommand => this.viewDetailsCommand ??
            (this.viewDetailsCommand = new DelegateCommand(ExecuteViewDetailsCommand));

        #endregion Properties

        #region Methods

        private static void ExecuteViewDetailsCommand()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ShowDetailsEventArgs<Common.Models.Item>(true));
        }

        #endregion Methods
    }
}
