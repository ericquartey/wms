using System.ComponentModel;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;
using Microsoft.Practices.ServiceLocation;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TEntity> : BindableBase, IWmsGridViewModel where TEntity : class
    {
        #region Fields

        private readonly IDataService dataService = ServiceLocator.Current.GetInstance<IDataService>();
        private readonly BindingList<TEntity> items = new BindingList<TEntity>();
        private IFilter currentFilter;
        private TEntity selectedItem;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                          .Subscribe<RefreshItemsEvent<TEntity>>(eventArgs => this.RefreshGrid(), true);
            this.RefreshGrid();
        }

        #endregion Constructors

        #region Properties

        public IFilter CurrentFilter
        {
            get => this.currentFilter;
            set
            {
                if (this.SetProperty(ref this.currentFilter, value))
                {
                    this.NotifyFilterChanged();
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
            this.Items.RaiseListChangedEvents = false;
            var elements = this.dataService.GetData<TEntity>();

            this.items.Clear();
            foreach (var item in elements)
            {
                this.items.Add(item);
            }

            this.Items.RaiseListChangedEvents = true;
            this.Items.ResetBindings();
        }

        protected void NotifyFilterChanged()
        {
            this.RefreshGrid();
        }

        protected void NotifySelectionChanged()
        {
            ServiceLocator.Current.GetInstance<IEventService>()
                .Invoke(new ItemSelectionChangedEvent<TEntity>(this.selectedItem));
        }

        #endregion Methods
    }
}
