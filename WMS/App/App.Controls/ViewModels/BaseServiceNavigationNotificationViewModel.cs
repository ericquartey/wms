using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.Utils;
using Ferretto.WMS.App.Controls.Services;

namespace Ferretto.WMS.App.Controls
{
    public abstract class BaseServiceNavigationNotificationViewModel : BaseServiceNavigationViewModel
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

        protected abstract Task LoadDataAsync(ModelChangedPubSubEvent e);

        protected override void OnDispose()
        {
            if (this.modelChangedEventSubscription != null)
            {
                this.EventService.Unsubscribe<ModelChangedPubSubEvent>(this.modelChangedEventSubscription);
            }

            base.OnDispose();
        }

        private void SubscribeToEvents()
        {
            var attributes = this.GetType()
                .GetCustomAttributes(typeof(ResourceAttribute), true)
                .Cast<ResourceAttribute>();

            if (attributes.Any())
            {
                this.modelChangedEventSubscription = this.EventService
                    .Subscribe<ModelChangedPubSubEvent>(
                        async eventArgs => { await this.LoadDataAsync(eventArgs).ConfigureAwait(true); },
                        true,
                        e => attributes.Any(a =>
                            a.ResourceName == e.ResourceName));
            }
        }

        #endregion
    }
}
