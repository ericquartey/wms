using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls;
using Ferretto.Common.Utils;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.WMS.Modules.MasterData
{
    public class EntityListViewModel<TModel> : BaseServiceNavigationViewModel
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly IEnumerable<IDataSource<TModel>> dataSources;
        private IEnumerable<Tile> filterTiles;
        private IDataSource<TModel> selectedDataSource;
        private Tile selectedFilterTile;
        private object selectedItem;
        private ICommand viewDetailsCommand;

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

        public TModel CurrentItem
        {
            get
            {
                if (this.selectedItem == null)
                {
                    return default(TModel);
                }
                return (TModel)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedItem).OriginalRow);
            }
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
                    this.SelectedDataSource = this.dataSources.Single(dataSource => dataSource.Name == value.Name);
                }
            }
        }

        public object SelectedItem
        {
            get => this.selectedItem;
            set => this.SetProperty(ref this.selectedItem, value);
        }

        public ICommand ViewDetailsCommand => this.viewDetailsCommand ??
                        (this.viewDetailsCommand = new DelegateCommand(this.ExecuteViewDetailsCommand));

        #endregion Properties

        #region Methods

        public virtual void ExecuteViewDetailsCommand()
        {
            // Nothing to do here.
            // The derived classes can override this method to impelement the ViewDetails command behaviour.
        }

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
