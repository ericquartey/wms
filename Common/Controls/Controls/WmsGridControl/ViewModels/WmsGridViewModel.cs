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
        private TEntity selectedItem;
        private IFilter currentFilter;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {            
            this.dataService = ServiceLocator.Current.GetInstance<IDataService>();
            ServiceLocator.Current.GetInstance<IEventService>()
                          .Subscribe<RefreshItemsEvent<TEntity>>(eventArgs => this.RefreshGrid(), true);
            this.RefreshGrid();
        }

        #endregion Constructors

        #region Properties

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

        protected void NotifySelectionChanged()
        {
            // TODO ServiceLocator.Current.GetInstance<IEventService>().Invoke(new ItemSelectionChangedEvent<TModel>(this.selectedItem));
        }

        protected void NotifyFilterChanged()
        {
            this.Items.Clear();
            this.RefreshGrid();
        }

        #endregion Methods
    }
}
