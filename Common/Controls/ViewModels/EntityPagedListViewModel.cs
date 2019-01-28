using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.Controls
{
    public class EntityPagedListViewModel<TModel> : EntityListViewModel<TModel>
            where TModel : IBusinessObject
    {
        #region Fields

        private const int DefaultPageSize = 30;

        private CriteriaOperator customFilter;

        private object dataSource;

        private CriteriaOperator overallFilter;

        private IPagedBusinessProvider<TModel> provider;

        private string searchText;

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

        protected override void OnDispose()
        {
            base.OnDispose();

            (this.dataSource as InfiniteAsyncSource)?.Dispose();
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

        private void ComputeOverallFilter()
        {
            var filterDataSource = this.FilterDataSources.Single(d => d.Key == this.selectedFilterTile.Key);

            this.Provider = filterDataSource.Provider;

            var overallFilter = CriteriaOperator.TryParse(filterDataSource.Expression);
            var searchTextOperator = CriteriaOperator.TryParse(this.searchText);
            if (searchTextOperator is null == false)
            {
                if (overallFilter is null == false)
                {
                    overallFilter = CriteriaOperator.And(overallFilter, searchTextOperator);
                }
                else
                {
                    overallFilter = searchTextOperator;
                }
            }

            if (this.CustomFilter is null == false)
            {
                if (overallFilter is null == false)
                {
                    overallFilter = CriteriaOperator.And(overallFilter, this.CustomFilter);
                }
                else
                {
                    overallFilter = this.CustomFilter;
                }
            }

            this.OverallFilter = overallFilter;
        }

        private async Task<FetchRowsResult> FetchRowsAsync(FetchRowsAsyncEventArgs e)
        {
            var orderBy = GetSortOrder(e);
            var filterExpression = e.Filter.BuildExpression();
            IExpression where = null;
            IExpression search = null;

            if (filterExpression is BinaryExpression expression)
            {
                if (expression.LeftExpression is ValueExpression valueExpressionLeft
                    &&
                    expression.RightExpression is ValueExpression valueExpressionRight)
                {
                    where = expression;
                }
            }
            else if (filterExpression is ValueExpression valueExpression)
            {
                search = valueExpression;
            }

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

        #endregion Methods
    }
}
