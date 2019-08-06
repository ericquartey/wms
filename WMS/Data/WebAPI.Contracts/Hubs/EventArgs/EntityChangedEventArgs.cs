using System;
using Ferretto.WMS.Data.Hubs.Models;

namespace Ferretto.WMS.Data.WebAPI.Contracts
{
    public sealed class EntityChangedEventArgs : EventArgs
    {
        #region Constructors

        public EntityChangedEventArgs(string entityType, string id, HubEntityOperation operation, string sourceEntityType, string sourceId)
        {
            this.EntityType = entityType;
            this.Id = id;
            this.Operation = operation;
            this.SourceEntityType = sourceEntityType;
            this.SourceId = sourceId;
        }

        #endregion

        #region Properties

        public string EntityType { get; private set; }

        public string Id { get; private set; }

        public HubEntityOperation Operation { get; private set; }

        public string SourceEntityType { get; private set; }

        public string SourceId { get; private set; }

        #endregion
    }
}
