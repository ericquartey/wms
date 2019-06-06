using System;
using Ferretto.WMS.Data.Hubs;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public sealed class EntityChangedEventArgs : EventArgs
    {
        #region Constructors

        public EntityChangedEventArgs(string entityType, string id, HubEntityOperation operation)
        {
            this.EntityType = entityType;
            this.Id = id;
            this.Operation = operation;
        }

        #endregion

        #region Properties

        public string EntityType { get; private set; }

        public string Id { get; private set; }

        public HubEntityOperation Operation { get; private set; }

        #endregion
    }
}
