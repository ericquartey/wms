using System.ComponentModel;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TEntity> : BindableBase, IWmsGridViewModel where TEntity : IBusinessObject
    {
        #region Fields

        private readonly IEventService eventService = ServiceLocator.Current.GetInstance<IEventService>();
        private readonly BindingList<TEntity> items = new BindingList<TEntity>();
        private IDataSource<TEntity> currentDataSource;
        private TEntity selectedItem;

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

        public BindingList<TEntity> Items => this.items;

        public TEntity SelectedItem
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
            if (this.CurrentDataSource == null)
            {
                return;
            }

            this.Items.RaiseListChangedEvents = false;
            var elements = this.CurrentDataSource.Load();

            this.items.Clear();
            foreach (var item in elements)
            {
                this.items.Add(item);
            }

            this.Items.RaiseListChangedEvents = true;
            this.Items.ResetBindings();
        }

        public void SetDataSource(object dataSource)
        {
            this.CurrentDataSource = dataSource as IDataSource<TEntity>;
        }

        protected void NotifyDataSourceChanged()
        {
            this.RefreshGrid();
        }

        protected void NotifySelectionChanged()
        {
            this.eventService.Invoke(new ItemSelectionChangedEvent<TEntity>(this.selectedItem));
        }

        #endregion Methods
    }
}
