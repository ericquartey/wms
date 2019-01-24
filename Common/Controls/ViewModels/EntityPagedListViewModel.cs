using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Data.Filtering;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.BusinessProviders;
using Ferretto.Common.Utils.Expressions;

namespace Ferretto.Common.Controls
{
    public class EntityPagedListViewModel<TModel> : EntityListViewModel<TModel>
            where TModel : IBusinessObject
    {
        #region Fields

        private const int DefaultPageSize = 30;

        private object dataSource;

        private IPagedBusinessProvider<TModel> provider;

        #endregion Fields

        #region Properties

        public IPagedBusinessProvider<TModel> Provider
        {
            get => this.provider;
            set => this.SetProperty(ref this.provider, value);
        }

        public override Tile SelectedFilter
        {
            get => this.selectedFilterTile;
            set
            {
                if (this.SetProperty(ref this.selectedFilterTile, value))
                {
                    // TODO: use later // var filterDataSource = this.FilterDataSources.Single(d => d.Key == value.Key);
                    this.SelectedFilterDataSource = this.InitializeSource();
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

        private static void GetUniqueValues(GetUniqueValuesAsyncEventArgs e)
        {
            var propertyInfo = typeof(TModel).GetProperty(e.PropertyName);
            if (propertyInfo == null
               ||
                !propertyInfo.PropertyType.IsEnum)
            {
                throw new InvalidOperationException();
            }

            var values = Enum.GetValues(propertyInfo.PropertyType).Cast<object>().ToArray();
            e.Result = Task.FromResult(values);
        }

        private async Task<FetchRowsResult> FetchRowsAsync(FetchRowsAsyncEventArgs e)
        {
            var orderBy = GetSortOrder(e);
            var filterExpression = e.Filter.BuildExpression();
            IExpression where = null;
            IExpression search = null;

            if (filterExpression is BinaryExpression expression)
            {
                if (expression.LeftExpression is ValueExpression valueExpressionLeft)
                {
                    where = expression.RightExpression;
                    search = valueExpressionLeft;
                }
                else if (expression.RightExpression is ValueExpression valueExpressionRight)
                {
                    where = expression.LeftExpression;
                    search = valueExpressionRight;
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

        /*
        private async Task<object[]> GetTotalSummariesAsync(GetSummariesAsyncEventArgs e)
        {
            var filter = BuildExpression(e.Filter);

            return e.Summaries.Select(s =>
            {
                switch (s.SummaryType)
                {
                    case SummaryType.Count:
                        {
                            return (object)this.provider.GetAllCount();
                        }
                    default:
                        {
                            throw new InvalidOperationException();
                        }
                }
            }).ToArray();
        }
        */

        private InfiniteAsyncSource InitializeSource()
        {
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
                GetUniqueValues(e);
            };

            /*
            source.GetTotalSummaries += (o, e) =>
            {
                e.Result = this.GetTotalSummariesAsync(e);
            };
            */

            return source;
        }

        #endregion Methods

        /*
        private void TableView_SearchStringToFilterCriteria(object sender, DevExpress.Xpf.Grid.SearchStringToFilterCriteriaEventArgs e)
        {
            e.Filter = new ConstantValue(e.SearchString);
            // e.ApplyToColumnsFilter = true;
        }
        */
        /*
        private void tileBar_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = this.tileBar.SelectedItem as Tile;

            if (item?.Key == "ItemsViewAll")
            {
                this.MainGridControl.FixedFilter = null;
            }
            else if (item?.Key == "ItemsViewClassA")
            {
                this.MainGridControl.FixedFilter = CriteriaOperator.Parse("[AbcClassDescription] == 'A Class'");
            }
            else if (item?.Key == "ItemsViewFIFO")
            {
                this.MainGridControl.FixedFilter = CriteriaOperator.Parse("[ManagementTypeDescription] == 'FIFO'");
            }
        }
        */
    }
}
