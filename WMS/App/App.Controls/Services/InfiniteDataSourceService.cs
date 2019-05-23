﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpf.Data;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.BLL.Interfaces.Providers;
using Ferretto.Common.Utils.Expressions;
using Prism.Mvvm;

namespace Ferretto.WMS.App.Controls.Services
{
    public class InfiniteDataSourceService<TModel, TKey> : BindableBase
        where TModel : IModel<TKey>
    {
        #region Fields

        private const int DefaultPageSize = 30;

        private const string DefaultPageSizeSettingsKey = "DefaultListPageSize";

        private readonly Func<int, int, IEnumerable<SortOption>, Task<IEnumerable<TModel>>> getAllAsync;

        private InfiniteAsyncSource dataSource;

        private IPagedBusinessProvider<TModel, TKey> provider;

        #endregion

        #region Constructors

        public InfiniteDataSourceService(IPagedBusinessProvider<TModel, TKey> provider, Func<int, int, IEnumerable<SortOption>, Task<IEnumerable<TModel>>> getAll = null)
        {
            this.Provider = provider;
            this.getAllAsync = getAll;
        }

        #endregion

        #region Properties

        public InfiniteAsyncSource DataSource
        {
            get => this.dataSource;
            set => this.SetProperty(ref this.dataSource, value);
        }

        public IPagedBusinessProvider<TModel, TKey> Provider
        {
            get => this.provider;
            set
            {
                if (this.SetProperty(ref this.provider, value))
                {
                    this.DataSource = this.InitializeSource();
                }
            }
        }

        #endregion

        #region Methods

        public async Task<object[]> GetUniqueValuesAsync(string propertyName)
        {
            return (await this.Provider.GetUniqueValuesAsync(propertyName)).ToArray();
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

        private async Task<FetchRowsResult> FetchRowsAsync(FetchRowsAsyncEventArgs e)
        {
            try
            {
                IEnumerable<TModel> entities = null;
                if (this.getAllAsync != null)
                {
                    entities = await this.getAllAsync(e.Skip, GetPageSize(), GetSortOrder(e));
                }
                else
                {
                    entities = await this.provider.GetAllAsync(e.Skip, GetPageSize(), GetSortOrder(e));
                }

                return new FetchRowsResult(entities.Cast<object>().ToArray(), hasMoreRows: entities.Count() == GetPageSize());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message.Split(new[] { '\n', '\r' })[0]);
            }
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
