using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using CommonServiceLocator;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public class EntityListViewModel<TModel, TKey> : BaseServiceNavigationViewModel, IEntityListViewModel
        where TModel : IModel<TKey>
    {
        #region Fields

        protected Tile selectedFilterTile;

        private ICommand addCommand;

        private IEnumerable<IFilterDataSource<TModel, TKey>> filterDataSources;

        private IEnumerable<Tile> filterTiles;

        private bool flattenDataSource;

        private object modelChangedEventSubscription;

        private object modelRefreshSubscription;

        private ICommand refreshCommand;

        private object selectedFilterDataSource;

        private object selectedItem;

        #endregion

        #region Constructors

        protected EntityListViewModel()
        {
            this.InitializeEvent();
        }

        #endregion

        #region Properties

        public ICommand AddCommand => this.addCommand ??
              (this.addCommand = new DelegateCommand(this.ExecuteAddCommand));

        public ColorRequired ColorRequired => ColorRequired.Default;

        public TModel CurrentItem
        {
            get
            {
                if (this.selectedItem == null)
                {
                    return default(TModel);
                }

                if (this.selectedItem is TModel)
                {
                    return (TModel)this.selectedItem;
                }

                if ((this.selectedItem is DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread) == false)
                {
                    return default(TModel);
                }

                return (TModel)((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedItem).OriginalRow;
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

        public ICommand RefreshCommand => this.refreshCommand ??
               (this.refreshCommand = new DelegateCommand(
               this.ExecuteRefreshCommand));

        public virtual Tile SelectedFilter
        {
            get => this.selectedFilterTile;
            set
            {
                if (this.SetProperty(ref this.selectedFilterTile, value))
                {
                    var filterDataSource = this.filterDataSources.Single(d => d.Key == value.Key);
                    this.SelectedFilterDataSource = this.flattenDataSource ? filterDataSource.GetData() : (object)filterDataSource;
                }
            }
        }

        public virtual object SelectedFilterDataSource
        {
            get => this.selectedFilterDataSource;
            protected set => this.SetProperty(ref this.selectedFilterDataSource, value);
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

        protected IEnumerable<IFilterDataSource<TModel, TKey>> FilterDataSources => this.filterDataSources;

        #endregion

        #region Methods

        public virtual void LoadRelatedData()
        {
            var oldFilterDataSource = this.selectedFilterDataSource;
            this.SelectedFilterDataSource = null;
            this.SelectedFilterDataSource = oldFilterDataSource;
        }

        public virtual async Task UpdateFilterTilesCountsAsync()
        {
            await Task.Run(() =>
            {
                foreach (var filterTile in this.filterTiles)
                {
                    filterTile.Count = this.filterDataSources.Single(d => d.Key == filterTile.Key).GetDataCount?.Invoke();
                }
            }).ConfigureAwait(true);
        }

        protected virtual void ExecuteAddCommand()
        {
        }

        protected void ExecuteRefreshCommand()
        {
            this.LoadRelatedData();
        }

        protected override async Task OnAppearAsync()
        {
            // TODO: check cycle because OnAppear is Async
            // await base.OnAppearAsync();

            try
            {
                var dataSourceService = ServiceLocator.Current.GetInstance<IDataSourceService>();
                this.filterDataSources = dataSourceService.GetAllFilters<TModel, TKey>(this.GetType().Name, this.Data);
                this.filterTiles = new BindingList<Tile>(this.filterDataSources.Select(filterDataSource => new Tile
                {
                    Key = filterDataSource.Key,
                    Name = filterDataSource.Name
                }).ToList());

                await this.UpdateFilterTilesCountsAsync().ConfigureAwait(true);
            }
            catch (System.Exception ex)
            {
                this.EventService.Invoke(new StatusPubSubEvent(ex));
            }
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsPubSubEvent<TModel>>(this.modelRefreshSubscription);
            this.EventService.Unsubscribe<ModelChangedPubSubEvent<TModel, TKey>>(this.modelChangedEventSubscription);

            base.OnDispose();
        }

        private void InitializeEvent()
        {
            this.modelRefreshSubscription = this.EventService.Subscribe<RefreshModelsPubSubEvent<TModel>>(eventArgs => { this.LoadRelatedData(); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedPubSubEvent<TModel, TKey>>(eventArgs => { this.LoadRelatedData(); });
        }

        #endregion
    }
}
