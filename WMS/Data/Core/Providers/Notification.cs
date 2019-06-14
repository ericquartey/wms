using System;
using Ferretto.WMS.Data.Hubs.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    public sealed class Notification
    {
        #region Constructors

        public Notification(string modelId, Type modelType, HubEntityOperation operationType)
        {
            this.ModelId = modelId;
            this.ModelType = modelType;
            this.OperationType = operationType;
        }

        #endregion

        #region Properties

        public string ModelId { get; }

        public Type ModelType { get; }

        public HubEntityOperation OperationType { get; }

        #endregion

        #region Methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((Notification)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = this.ModelId != null ? this.ModelId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (this.ModelType != null ? this.ModelType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)this.OperationType;
                return hashCode;
            }
        }

        private bool Equals(Notification other)
        {
            return string.Equals(this.ModelId, other.ModelId, StringComparison.Ordinal) &&
                this.ModelType == other.ModelType &&
                this.OperationType == other.OperationType;
        }

        #endregion
    }
}
