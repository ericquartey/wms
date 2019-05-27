using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public abstract class BaseServiceNavigationNotificationViewModel<TModel, TKey> : BaseServiceNavigationViewModel
        where TModel : IModel<TKey>
    {
        #region Fields

        private object modelChangedEventSubscription;

        #endregion

        #region Constructors

        protected BaseServiceNavigationNotificationViewModel()
        {
            this.SubscribeToEvents();
        }

        #endregion

        #region Methods

        protected abstract Task LoadDataAsync();

        protected override void OnDispose()
        {
            this.EventService.Unsubscribe<ModelChangedPubSubEvent>(this.modelChangedEventSubscription);

            base.OnDispose();
        }

        private void SubscribeToEvents()
        {
            var attribute = typeof(TModel)
                .GetCustomAttributes(typeof(ResourceAttribute), true)
                .FirstOrDefault() as ResourceAttribute;

            if (attribute != null)
            {
                this.modelChangedEventSubscription = this.EventService
                    .Subscribe<ModelChangedPubSubEvent>(
                        async eventArgs => { await this.LoadDataAsync().ConfigureAwait(true); },
                        false,
                        e => e.ResourceName == attribute.ResourceName);
            }
        }

        #endregion
    }
}
