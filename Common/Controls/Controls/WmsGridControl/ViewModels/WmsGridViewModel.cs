using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TEntity> : BaseNavigationViewModel, IWmsGridViewModel where TEntity : IBusinessObject
    {
        #region Fields

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private IDataSource<TEntity> currentDataSource;
        private object selectedItem;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {
            this.eventService.Subscribe<RefreshItemsEvent<TEntity>>(eventArgs => this.RefreshGrid(), true);
        }

        #endregion Constructors

        #region Properties

        public IDataSource<TEntity> CurrentDataSource
        {
            get => this.currentDataSource;
            set
            {
                if (this.SetProperty(ref this.currentDataSource, value))
                {
                    this.NotifyDataSourceChanged();
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
                    this.NotifySelectionChanged();
                }
            }
        }

        #endregion Properties

        #region Methods

        public void RefreshGrid()
        {
            // do nothing
        }

        public void SetDataSource(object dataSource)
        {
            if (dataSource == null || dataSource is IDataSource<TEntity> dataSourceEntity)
            {
                this.CurrentDataSource = dataSource as IDataSource<TEntity>;
            }
            else
            {
                throw new System.ArgumentException("Data source is not of the right type", nameof(dataSource));
            }
        }

        protected void NotifyDataSourceChanged()
        {
            this.RefreshGrid();
        }

        protected void NotifySelectionChanged()
        {
            var selectedId = this.selectedItem?.GetType().GetProperty("Id")?.GetValue(this.selectedItem);
            this.eventService.Invoke(new ItemSelectionChangedEvent<TEntity>(selectedId, this.Token));
        }

        #endregion Methods
    }
}
