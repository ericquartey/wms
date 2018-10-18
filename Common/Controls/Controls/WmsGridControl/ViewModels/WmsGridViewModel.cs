using DevExpress.Data.Async.Helpers;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TModel> : BaseServiceNavigationViewModel, IWmsGridViewModel where TModel : class, IBusinessObject
    {
        #region Fields

        private readonly object refreshModelsEventSubscription;

        private object selectedItem;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {
            this.refreshModelsEventSubscription = this.EventService.Subscribe<RefreshModelsEvent<TModel>>(eventArgs => { }, true);
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
                var model = this.selectedItem as TModel;
                if (this.selectedItem is ReadonlyThreadSafeProxyForObjectFromAnotherThread proxyForObjectFromAnotherThread)
                {
                    model = proxyForObjectFromAnotherThread.OriginalRow as TModel;
                }

                if (model != null)
                {
                    selectedModelId = model.Id;
                }
            }

            this.EventService.Invoke(new ModelSelectionChangedEvent<TModel, int>(selectedModelId, this.Token));
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<TModel>>(this.refreshModelsEventSubscription);
            base.OnDispose();
        }

        #endregion Methods
    }
}
