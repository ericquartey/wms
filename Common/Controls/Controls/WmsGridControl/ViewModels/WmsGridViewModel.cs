using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TEntity, TId> : BaseServiceNavigationViewModel, IWmsGridViewModel where TEntity : IBusinessObject
    {
        #region Fields

        private readonly object refreshModelsEventSubscription;

        private object selectedItem;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {
            this.refreshModelsEventSubscription = this.EventService.Subscribe<RefreshModelsEvent<TEntity>>(eventArgs => { }, true);
        }

        #endregion Constructors

        #region Properties

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

        protected void NotifySelectionChanged()
        {
            var selectedModelId = 0;
            if (this.selectedItem != null && this.selectedItem is DevExpress.Data.NotLoadedObject == false)
            {
                var model = (TEntity)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedItem).OriginalRow);
                if (model != null)
                {
                    selectedModelId = model.Id;
                }
            }

            this.EventService.Invoke(new ModelSelectionChangedEvent<TEntity, int>(selectedModelId, this.Token));
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<TEntity>>(this.refreshModelsEventSubscription);
            base.OnDispose();
        }

        #endregion Methods
    }
}
