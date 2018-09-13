using System;
using Ferretto.Common.Controls.Interfaces;
using Prism.Mvvm;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel : BindableBase, IWmsGridViewModel
    {
        // TODO private readonly IEntityService<TModel, TId> entityService;
        // TODO private IEnumerable<Object> items;

        #region Fields

        private Object selectedItem;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {
            // TODO this.entityService = ServiceLocator.Current.GetInstance<IEntityService<TModel, TId>>();

            this.Initialize();
        }

        #endregion Constructors

        // TODO public IEnumerable<TModel> Items => this.items;

        #region Properties

        public Object SelectedItem
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
            // TODO this.items = this.entityService.GetAll();
        }

        protected void NotifySelectionChanged()
        {
            // TODO ServiceLocator.Current.GetInstance<IEventService>().Invoke(new ItemSelectionChangedEvent<TModel>(this.selectedItem));
        }

        private void Initialize()
        {
            this.RefreshGrid();
        }

        #endregion Methods
    }
}
