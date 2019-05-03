using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.WMS.App.Controls
{
    public class EntityPagedListViewModel<TModel, TKey> : EntityListViewModel<TModel, TKey>
        where TModel : IModel<TKey>, IPolicyDescriptor<IPolicy>
    {
        #region Fields

        private const int DefaultPageSize = 30;

        private const string DefaultPageSizeSettingsKey = "DefaultListPageSize";

        private CriteriaOperator customFilter;

        private object dataSource;

        private CriteriaOperator overallFilter;

        private IPagedBusinessProvider<TModel, TKey> provider;

        private string searchText;

        private Tile selectedFilterTile;

        #endregion

        #region Constructors

        protected EntityPagedListViewModel()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the filter set by the filter editor.
        /// </summary>
        public CriteriaOperator CustomFilter
        {
            get => this.customFilter;
            set
            {
                if (this.SetProperty(ref this.customFilter, value))
                {
                    this.ComputeOverallFilter();
                }
            }
        }

        /// <summary>
        /// Gets or sets the fixed filter of the grid.
        /// It is used to filter the data based on the filter tiles.
        /// </summary>
        public CriteriaOperator OverallFilter
        {
            get => this.overallFilter;
            set => this.SetProperty(ref this.overallFilter, value);
        }

        public IPagedBusinessProvider<TModel, TKey> Provider
        {
            get => this.provider;
            set
            {
                if (this.SetProperty(ref this.provider, value))
                {
                    this.SelectedFilterDataSource = this.InitializeSource();
                }
            }
        }

        [Display(Name = nameof(Ferretto.Common.Resources.DesktopApp.EmptyString), ResourceType = typeof(Ferretto.Common.Resources.DesktopApp))]
        public string SearchText
        {
            get => this.searchText;
            set
            {
                if (this.SetProperty(ref this.searchText, value))
                {
                    this.ComputeOverallFilter();
                }
            }
        }

        public override Tile SelectedFilter
        {
            get => this.selectedFilterTile;
            set
            {
                if (this.SetProperty(ref this.selectedFilterTile, value))
                {
                    this.ComputeOverallFilter();
                }
            }
        }

        public override object SelectedFilterDataSource
        {
            get => this.dataSource;
            protected set => this.SetProperty(ref this.dataSource, value);
        }

        #endregion

        #region Methods

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return (await this.Provider.GetUniqueValuesAsync(propertyName)).ToArray();
        }

        public override void LoadRelatedData()
        {
            this.LoadDataAsync();
        }

        public override async Task UpdateFilterTilesCountsAsync()
        {
            foreach (var filterTile in this.Filters)
            {
                var filterDataSource = this.FilterDataSources.Single(d => d.Key == filterTile.Key);

                if (filterDataSource.Provider != null)
                {
                    filterTile.Count = await filterDataSource.Provider.GetAllCountAsync(filterDataSource.FilterString);
                }
            }
        }

        protected virtual void ExecuteShowFiltersCommand()
        {
            // do nothing: derived classes can customize the behaviour of this command
        }

        protected override Task LoadDataAsync()
        {
            Application.Current.Dispatcher.InvokeAsync(
                async () =>
            {
                (this.dataSource as InfiniteAsyncSource)?.RefreshRows();
                (this.dataSource as InfiniteAsyncSource)?.UpdateSummaries();
                await this.UpdateFilterTilesCountsAsync();
            }, DispatcherPriority.Normal);

            return Task.CompletedTask;
        }

        protected override void OnDispose()
        {
            (this.dataSource as InfiniteAsyncSource)?.Dispose();

            base.OnDispose();
        }

        private static int GetPageSize()
        {
            var pageSizeSetting = ConfigurationManager.AppSettings.Get(DefaultPageSizeSettingsKey);
            if (int.TryParse(pageSizeSetting, out var pageSize))
            {
                return pageSize;
            }

            return DefaultPageSize;
        }

        private static IEnumerable<SortOption> GetSortOrder(FetchRowsAsyncEventArgs e)
        {
            return e?.SortOrder.Select(s => new SortOption(s.PropertyName, s.Direction));
        }

        private static CriteriaOperator JoinFilters(CriteriaOperator operator1, CriteriaOperator operator2)
        {
            if (operator1 is null == false && operator2 is null == false)
            {
                return CriteriaOperator.And(operator1, operator2);
            }

            return operator1 ?? operator2;
        }

        private void ComputeOverallFilter()
        {
            var filterDataSource = this.FilterDataSources.Single(d => d.Key == this.selectedFilterTile.Key);

            this.Provider = filterDataSource.Provider;

            var newOverallFilter = CriteriaOperator.TryParse(filterDataSource.FilterString);

            this.OverallFilter = JoinFilters(newOverallFilter, this.customFilter);
            (this.dataSource as InfiniteAsyncSource)?.RefreshRows();
            (this.dataSource as InfiniteAsyncSource)?.UpdateSummaries();
        }

        private async Task<FetchRowsResult> FetchRowsAsync(FetchRowsAsyncEventArgs e)
        {
            var orderBySortOptions = GetSortOrder(e);

            var whereString = this.overallFilter?.ToString();

            IEnumerable<TModel> entities = null;

            string searchTextStarting = null;
            do
            {
                searchTextStarting = this.searchText;

                entities = await this.provider.GetAllAsync(
                    e.Skip,
                    GetPageSize(),
                    orderBySortOptions,
                    whereString,
                    searchTextStarting);
            }
            while (searchTextStarting != this.searchText);

            return new FetchRowsResult(
                entities.Cast<object>().ToArray(),
                hasMoreRows: entities.Count() == GetPageSize());
        }

        private void GetTotalSummaries(GetSummariesAsyncEventArgs e)
        {
            e.Result = this.GetTotalSummariesAsync();
        }

        private async Task<object[]> GetTotalSummariesAsync()
        {
            var whereString = this.overallFilter?.ToString();

            return new object[]
            {
                await this.Provider.GetAllCountAsync(
                    whereString,
                    this.searchText)
            };
        }

        private void GetUniqueValues(GetUniqueValuesAsyncEventArgs e)
        {
            var propertyInfo = typeof(TModel).GetProperty(e.PropertyName);
            if (propertyInfo != null)
            {
                if (propertyInfo.PropertyType.IsEnum)
                {
                    var values = Enum.GetValues(propertyInfo.PropertyType).Cast<object>().ToArray();
                    e.Result = Task.FromResult(values);
                }
                else
                {
                    e.Result = this.GetUniqueValuesAsync(propertyInfo.Name);
                }
            }
        }

        private InfiniteAsyncSource InitializeSource()
        {
            var source = new InfiniteAsyncSource
            {
                ElementType = typeof(TModel)
            };

            if (this.provider != null)
            {
                source.FetchRows += (o, e) => { e.Result = this.FetchRowsAsync(e); };

                source.GetUniqueValues += (o, e) => { this.GetUniqueValues(e); };

                source.GetTotalSummaries += (o, e) => { this.GetTotalSummaries(e); };
            }

            return source;
        }

        #endregion
    }
}
