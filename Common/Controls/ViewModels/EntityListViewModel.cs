using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class EntityListViewModel<TModel> : BaseServiceNavigationViewModel, IEntityListViewModel
        where TModel : IBusinessObject
    {
        #region Fields

        private readonly IEnumerable<ITileDataSource<TModel>> tileDataSources;
        private IEnumerable<Tile> filterTiles;
        private bool flattenDataSource;
        private object modelChangedEventSubscription;
        private object modelRefreshSubscription;
        private object selectedTileDataSource;
        private Tile selectedFilterTile;
        private object selectedItem;

        #endregion Fields

        #region Constructors

        protected EntityListViewModel()
        {
            var dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
            this.tileDataSources = dataSourceService.GetAllTiles<TModel>(this.GetType().Name);
            this.InitializeEvent();
            this.filterTiles = new BindingList<Tile>(this.tileDataSources.Select(tileDataSource => new Tile
            {
                Key = tileDataSource.Key,
                Name = tileDataSource.Name
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

        public object SelectedTileDataSource
        {
            get => this.selectedTileDataSource;
            protected set => this.SetProperty(ref this.selectedTileDataSource, value);
        }

        public Tile SelectedFilter
        {
            get => this.selectedFilterTile;
            set
            {
                if (this.SetProperty(ref this.selectedFilterTile, value))
                {
                    var tileDataSource = this.tileDataSources.Single(d => d.Key == value.Key);
                    this.SelectedTileDataSource = this.flattenDataSource ? tileDataSource.GetData() : (object)tileDataSource;
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

        #endregion Properties

        #region Methods

        public void RefreshData()
        {
            var oldTileDataSource = this.selectedTileDataSource;
            this.SelectedTileDataSource = null;
            this.SelectedTileDataSource = oldTileDataSource;
        }

        public async Task UpdateFilterTilesCountsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var filterTile in this.filterTiles)
                {
                    filterTile.Count = this.tileDataSources.Single(d => d.Key == filterTile.Key).GetDataCount();
                }
            }).ConfigureAwait(true);
        }

        protected override async void OnAppear()
        {
            base.OnAppear();
            await this.UpdateFilterTilesCountsAsync();
        }

        protected override void OnDisappear()
        {
            base.OnDisappear();
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<TModel>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<TModel>>(this.modelChangedEventSubscription);
            base.OnDispose();
        }

        private void InitializeEvent()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsEvent<TModel>>(eventArgs => { this.RefreshData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<TModel>>(eventArgs => { this.RefreshData(); });
        }

        #endregion Methods
    }
}
