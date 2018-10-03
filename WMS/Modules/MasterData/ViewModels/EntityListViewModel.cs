using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class EntityListViewModel<TModel> : BaseNavigationViewModel
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly IEnumerable<IDataSource<TModel>> dataSources;
        private IEnumerable<Tile> filterTiles;
        private IDataSource<TModel> selectedDataSource;
        private Tile selectedFilterTile;

        #endregion Fields

        #region Constructors

        protected EntityListViewModel()
        {
            var dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
            var viewName = MvvmNaming.GetViewNameFromViewModelName(this.GetType().Name);
            this.dataSources = dataSourceService.GetAll<TModel>(viewName);

            this.filterTiles = new BindingList<Tile>(this.dataSources.Select(dataSource => new Tile { Name = dataSource.Name }).ToList());
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<Tile> Filters
        {
            get => this.filterTiles;
            protected set => this.SetProperty(ref this.filterTiles, value);
        }

        public IDataSource<TModel> SelectedDataSource
        {
            get => this.selectedDataSource;
            protected set => this.SetProperty(ref this.selectedDataSource, value);
        }

        public Tile SelectedFilter
        {
            get => this.selectedFilterTile;
            set
            {
                if (this.SetProperty(ref this.selectedFilterTile, value))
                {
                    this.SelectedDataSource = this.dataSources.Single(dataSource => dataSource.Name == value.Name);
                }
            }
        }

        #endregion Properties

        #region Methods

        public async Task UpdateFilterTilesCountsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var filterTile in this.filterTiles)
                {
                    filterTile.Count = this.dataSources.Single(d => d.Name == filterTile.Name).GetDataCount();
                }
            }).ConfigureAwait(true);
        }

        #endregion Methods
    }
}
