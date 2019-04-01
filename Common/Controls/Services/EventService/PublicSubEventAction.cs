using System;
using Prism.Events;

namespace Ferretto.Common.Controls.Services
{
    public class PublicSubEventAction
    {
        #region Constructors

        public PublicSubEventAction(Action<object> actionToPublish, SubscriptionToken token)
        {
            this.ActionPublish = actionToPublish;
            this.Token = token;
        }

        #endregion

        #region Properties

        public Action<object> ActionPublish { get; set; }

        public SubscriptionToken Token { get; set; }

        #endregion
    }
}
