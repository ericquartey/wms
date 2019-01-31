using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Core.FilteringUI;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Services;
using Ferretto.Common.Utils.Expressions;
using NLog;
using Prism.Commands;

namespace Ferretto.Common.Controls
{
    public class EntityPagedListViewModel<TModel> : EntityListViewModel<TModel>
            where TModel : IBusinessObject
    {
        #region Fields

        private const int DefaultPageSize = 30;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private CriteriaOperator customFilter;

        private object dataSource;

        private object filteringChangedSubscription;

        private FilteringUIContext filteringContext;

        private CriteriaOperator overallFilter;

        private IPagedBusinessProvider<TModel> provider;

        private string searchText;

        private ICommand showFiltersCommand;

        #endregion Fields

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

        public FilteringUIContext FilteringContext
        {
            get => this.filteringContext;
            set => this.SetProperty(ref this.filteringContext, value);
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

        public IPagedBusinessProvider<TModel> Provider
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

        public ICommand ShowFiltersCommand => this.showFiltersCommand ??
             (this.showFiltersCommand = new DelegateCommand(this.ExecuteShowFiltersCommand));

        #endregion Properties

        #region Methods

        public override async Task UpdateFilterTilesCountsAsync()
        {
            foreach (var filterTile in this.Filters)
            {
                var filterDataSource = this.FilterDataSources.Single(d => d.Key == filterTile.Key);

                if (filterDataSource.Provider != null)
                {
                    string whereExpression = null;

                    if (filterDataSource.Expression != null)
                    {
                        whereExpression = CriteriaOperator.Parse(filterDataSource.Expression)
                            .BuildExpression()
                            .ToString();
                    }

                    filterTile.Count = await filterDataSource.Provider.GetAllCountAsync(whereExpression);
                }
            }
        }

        protected virtual void ExecuteShowFiltersCommand()
        {
            // do nothing: derived classes can customize the behaviour of this command
        }

        protected override void OnAppear()
        {
            base.OnAppear();

            this.filteringChangedSubscription = this.EventService.Subscribe<FilteringChangedPubSubEvent>(
                eventArgs => this.OnFilteringChanged(eventArgs), this.Token, true, true);
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<FilteringChangedPubSubEvent>(this.filteringChangedSubscription);
            (this.dataSource as InfiniteAsyncSource)?.Dispose();

            base.OnDispose();
        }

        private static IExpression BuildExpression(CriteriaOperator filter)
        {
            if (filter is ConstantValue constantValue)
            {
                return new ValueExpression(constantValue.Value.ToString());
            }
            else if (filter is OperandProperty operandProperty)
            {
                return new ValueExpression(operandProperty.PropertyName);
            }
            else if (filter is BinaryOperator binaryOperator)
            {
                return new BinaryExpression(binaryOperator.OperatorType.ToString())
                {
                    LeftExpression = BuildExpression(binaryOperator.LeftOperand),
                    RightExpression = BuildExpression(binaryOperator.RightOperand)
                };
            }
            else if (filter is GroupOperator groupOperator)
            {
                if (groupOperator.Operands.Count == 1)
                {
                    return BuildExpression(groupOperator.Operands.Single());
                }
                else
                {
                    return new BinaryExpression(groupOperator.OperatorType.ToString())
                    {
                        LeftExpression = BuildExpression(groupOperator.Operands.First()),
                        RightExpression = BuildExpression(
                            new GroupOperator(groupOperator.OperatorType, groupOperator.Operands.Skip(1)))
                    };
                }
            }

            return null;
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

            var newOverallFilter = CriteriaOperator.TryParse(filterDataSource.Expression);

            this.OverallFilter = JoinFilters(newOverallFilter, this.customFilter);
            this.logger.Debug($"Data source filter: '{this.OverallFilter}'");
            (this.dataSource as InfiniteAsyncSource)?.RefreshRows();
        }

        private async Task<FetchRowsResult> FetchRowsAsync(FetchRowsAsyncEventArgs e)
        {
            var orderBy = GetSortOrder(e);

            var where = this.overallFilter?.BuildExpression();
            var search = this.searchText?.BuildExpression();

            var entities = await this.provider.GetAllAsync(
                skip: e.Skip,
                take: DefaultPageSize,
                orderBy: orderBy,
                where: where?.ToString(),
                search: search?.ToString());

            return new FetchRowsResult(entities.Cast<object>().ToArray(), hasMoreRows: entities.Count() == DefaultPageSize);
        }

        private void GetTotalSummaries(GetSummariesAsyncEventArgs e)
        {
            e.Result = Task.FromResult(new object[] { 30, 20 });
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

        private async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return (await this.Provider.GetUniqueValuesAsync(propertyName)).ToArray();
        }

        private InfiniteAsyncSource InitializeSource()
        {
            if (this.provider == null)
            {
                return null;
            }

            var source = new InfiniteAsyncSource
            {
                ElementType = typeof(TModel)
            };

            source.FetchRows += (o, e) =>
            {
                e.Result = this.FetchRowsAsync(e);
            };

            source.GetUniqueValues += (o, e) =>
            {
                this.GetUniqueValues(e);
            };

            source.GetTotalSummaries += (o, e) =>
            {
                this.GetTotalSummaries(e);
            };

            return source;
        }

        private void OnFilteringChanged(FilteringChangedPubSubEvent eventArgs)
        {
            if (eventArgs.FilteringContext == this.filteringContext)
            {
                this.CustomFilter = eventArgs.Filter;
            }
        }

        #endregion Methods
    }
}
