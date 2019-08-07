using System;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Interfaces
{
    public interface INotificationService
    {
        #region Methods

        void PushCreate<TKeySource>(Type modelType, IModel<TKeySource> sourceModel);

        void PushCreate(Type modelType, string sourceModelId, Type sourceModelType);

        void PushCreate<TKey>(IModel<TKey> model);

        void PushCreate<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel);

        void PushDelete<TKeySource>(Type modelType, IModel<TKeySource> sourceModel);

        void PushDelete(Type modelType, string sourceModelId, Type sourceModelType);

        void PushDelete<TKey>(IModel<TKey> model);

        void PushDelete<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel);

        void PushUpdate<TKeySource>(Type modelType, IModel<TKeySource> sourceModel);

        void PushUpdate(Type modelType, string sourceModelId, Type sourceModelType);

        void PushUpdate<TKey>(IModel<TKey> model);

        void PushUpdate<TKey, TKeySource>(IModel<TKey> model, IModel<TKeySource> sourceModel);

        Task SendNotificationsAsync();

        void Clear();

        #endregion
    }
}
