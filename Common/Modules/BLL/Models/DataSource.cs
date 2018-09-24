using System;
using System.Linq;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Modules.BLL.Services;
using Prism.Mvvm;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class DataSource<TEntity> : BindableBase, IDataSource<TEntity> where TEntity : class
    {
        #region Fields

        private string countInfo;
        private bool isVisible;

        #endregion Fields

        #region Properties

        public int Count => this.GetTotalCount();

        public string CountInfo
        {
            get => this.countInfo;
            set => this.SetProperty(ref this.countInfo, value);
        }

        public string Description { get; set; }
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; set; }
        public Func<Func<IQueryable<TEntity>, IQueryable<TEntity>>, int> GetCount { get; set; }
        public Func<Func<IQueryable<TEntity>, IQueryable<TEntity>>, IQueryable<TEntity>> GetData { get; set; }

        public bool IsVisible
        {
            get => this.isVisible;
            set
            {
                this.SetProperty(ref this.isVisible, value);
                if (value)
                {
                    this.GetTotalCount();
                }
            }
        }

        public string Name { get; set; }
        public DataSourceType SourceName { get; set; }

        #endregion Properties

        #region Methods

        public int GetTotalCount()
        {
            if (this.GetCount == null)
            {
                this.CountInfo = "-";
                return 0;
            }
            var count = this.GetCount(this.Filter);
            this.CountInfo = count.ToString();

            return count;
        }

        public IQueryable<TEntity> Load()
        {
            return this.GetData?.Invoke(this.Filter);
        }

        public override String ToString()
        {
            return this.Name ?? base.ToString();
        }

        #endregion Methods
    }
}
