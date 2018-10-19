using System.Windows.Input;
using DevExpress.Data.Async.Helpers;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TModel> : BaseServiceNavigationViewModel, IWmsGridViewModel where TModel : class, IBusinessObject
    {
        #region Fields

        private object modelChangedEventSubscription;
        private object refreshModelsEventSubscription;
        private object selectedItem;

        #endregion Fields

        #region Constructors

        public WmsGridViewModel()
        {
        }

        #endregion Constructors

        #region Properties

        public ICommand CmdRefresh { get; set; }

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

            this.EventService.Invoke(new ModelSelectionChangedEvent<TModel>(selectedModelId, this.Token));
        }

        protected override void OnAppear()
        {
            base.OnAppear();
            this.refreshModelsEventSubscription = this.EventService.Subscribe<RefreshModelsEvent<TModel>>(eventArgs => { this.CmdRefresh?.Execute(null); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<TModel>>(eventArgs => { this.CmdRefresh?.Execute(null); });
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<TModel>>(this.refreshModelsEventSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<TModel>>(this.modelChangedEventSubscription);
            base.OnDispose();
        }

        #endregion Methods
    }
}
