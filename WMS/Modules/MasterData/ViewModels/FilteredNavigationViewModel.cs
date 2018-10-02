using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.WMS.Modules.MasterData
{
    public class FilteredNavigationViewModel<TModel> : BaseNavigationViewModel
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly IDataSourceService dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
        private IEnumerable<IDataSource<TModel>> dataSources;
        private IEnumerable<Tile> filterTiles;
        private IDataSource<TModel> selectedDataSource;
        private Tile selectedFilterTile;

        #endregion Fields

        #region Constructors

        protected FilteredNavigationViewModel()
        {
            var viewName = MvvmNaming.GetViewNameFromViewModelName(this.GetType().Name);
            this.dataSources = this.dataSourceService.GetAll(viewName) as IEnumerable<IDataSource<TModel>>;

            this.filterTiles = new BindingList<Tile>(this.DataSources.Select(dataSource => new Tile { Name = dataSource.Name }).ToList());
        }

        #endregion Constructors

        #region Properties

        public IEnumerable<IDataSource<TModel>> DataSources
        {
            get => this.dataSources;
            protected set => this.SetProperty(ref this.dataSources, value);
        }

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
                    this.SelectedDataSource = this.DataSources.Single(dataSource => dataSource.Name == value.Name);
                }
            }
        }

        #endregion Properties

        #region Methods

        public void UpdateFilterTilesCounts()
        {
            foreach (var filterTile in this.filterTiles)
            {
                filterTile.Count = this.DataSources.Single(dataSource => dataSource.Name == filterTile.Name).GetData().Count();
            }
        }

        #endregion Methods
    }
}
