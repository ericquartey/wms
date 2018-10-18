using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public class EntityListViewModel<TModel> : BaseServiceNavigationViewModel, IEntityListViewModel
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly IEnumerable<IDataSource<TModel>> dataSources;
        private IEnumerable<Tile> filterTiles;
        private bool flattenDataSource;
        private object selectedDataSource;
        private Tile selectedFilterTile;
        private object selectedItem;
        private ICommand viewDetailsCommand;

        #endregion Fields

        #region Constructors

        protected EntityListViewModel()
        {
            var dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
            this.dataSources = dataSourceService.GetAll<TModel>(this.GetType().Name);

            this.filterTiles = new BindingList<Tile>(this.dataSources.Select(dataSource => new Tile
            {
                Key = dataSource.Key,
                Name = dataSource.Name
            }).ToList());
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
                if ((this.selectedItem is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
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

        /// <summary>
        /// When set to True, skips the usage of the DevExpress InstantFeedbackSource.
        /// </summary>
        public bool FlattenDataSource
        {
            get => this.flattenDataSource;
            protected set => this.SetProperty(ref this.flattenDataSource, value);
        }

        public object SelectedDataSource
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
                    var dataSource = this.dataSources.Single(d => d.Key == value.Key);
                    this.SelectedDataSource = this.flattenDataSource ? dataSource.GetData() : (object)dataSource;
                }
            }
        }

        public object SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (this.SetProperty(ref this.selectedItem, value))
                {
                    this.RaisePropertyChanged(nameof(this.CurrentItem));
                }
            }
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
                    filterTile.Count = this.dataSources.Single(d => d.Key == filterTile.Key).GetDataCount();
                }
            }).ConfigureAwait(true);
        }

        protected override void OnAppear()
        {
            base.OnAppear();

            this.UpdateFilterTilesCountsAsync();
        }

        #endregion Methods
    }
}
