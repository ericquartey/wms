using System;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface INotificationService
    {
        #region Methods

        void PushCreate(Type modelType);

        void PushCreate<TKey>(IModel<TKey> model);

        void PushDelete(Type modelType);

        void PushDelete<TKey>(IModel<TKey> model);

        void PushUpdate(Type modelType);

        void PushUpdate<TKey>(IModel<TKey> model);

        Task SendNotificationsAsync();

        #endregion
    }
}
