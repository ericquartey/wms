using System.Windows.Input;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Controls.Interfaces;
using Ferretto.Common.Controls.Services;

namespace Ferretto.Common.Controls
{
    public class WmsGridViewModel<TEntity> : BaseServiceNavigationViewModel, IWmsGridViewModel where TEntity : IBusinessObject
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
                var model = (TEntity)(((DevExpress.Data.Async.Helpers.ReadonlyThreadSafeProxyForObjectFromAnotherThread)this.selectedItem).OriginalRow);
                if (model != null)
                {
                    selectedModelId = model.Id;
                }
            }

            this.EventService.Invoke(new ModelSelectionChangedEvent<TEntity>(selectedModelId, this.Token));
        }

        protected override void OnAppear()
        {
            base.OnAppear();
            this.refreshModelsEventSubscription = this.EventService.Subscribe<RefreshModelsEvent<TEntity>>(eventArgs => { this.CmdRefresh?.Execute(null); }, this.Token, true, true);
            this.modelChangedEventSubscription = this.EventService.Subscribe<ModelChangedEvent<TEntity>>(eventArgs => { this.CmdRefresh?.Execute(null); });
        }

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<RefreshModelsEvent<TEntity>>(this.refreshModelsEventSubscription);
            this.EventService.Unsubscribe<ModelChangedEvent<TEntity>>(this.modelChangedEventSubscription);
            base.OnDispose();
        }

        #endregion Methods
    }
}
