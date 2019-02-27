using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.Utils.Expressions;
using NLog;

namespace Ferretto.Common.Controls
{
    public class EntityPagedListViewModel<TModel, TKey> : EntityListViewModel<TModel, TKey>
       where TModel : IModel<TKey>
    {
        #region Fields

        private const int DefaultPageSize = 30;

        private const string DefaultPageSizeSettingsKey = "DefaultListPageSize";

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CriteriaOperator customFilter;

        private object dataSource;

        private CriteriaOperator overallFilter;

        private IPagedBusinessProvider<TModel, TKey> provider;

        private string searchText;

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

        [Display(Name = nameof(Resources.DesktopApp.EmptyString), ResourceType = typeof(Resources.DesktopApp))]
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
            (this.dataSource as InfiniteAsyncSource)?.RefreshRows();
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
            return e.SortOrder.Select(s => new SortOption(s.PropertyName, s.Direction));
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
            this.logger.Debug($"Data source filter: '{this.OverallFilter}'");
            (this.dataSource as InfiniteAsyncSource)?.RefreshRows();
        }

        private async Task<FetchRowsResult> FetchRowsAsync(FetchRowsAsyncEventArgs e)
        {
            var orderBySortOptions = GetSortOrder(e);

            var whereString = this.overallFilter?.ToString();

            var entities = await this.provider.GetAllAsync(
                e.Skip,
                GetPageSize(),
                orderBySortOptions,
                whereString,
                this.searchText);

            return new FetchRowsResult(entities.Cast<object>().ToArray(), hasMoreRows: entities.Count() == GetPageSize());
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
                source.FetchRows += (o, e) =>
                {
                    e.Result = this.FetchRowsAsync(e);
                };

                source.GetUniqueValues += (o, e) =>
                {
                    this.GetUniqueValues(e);
                };
            }

            return source;
        }

        #endregion
    }
}
