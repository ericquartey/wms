using System;
using Ferretto.WMS.Data.Hubs.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    public sealed class Notification
    {
        #region Constructors

        public Notification(string modelId, Type modelType, string sourceModelId, Type sourceModelType, HubEntityOperation operationType)
        {
            this.ModelId = modelId;
            this.ModelType = modelType;
            this.OperationType = operationType;
            this.SourceModelId = sourceModelId;
            this.SourceModelType = sourceModelType;
        }

        #endregion

        #region Properties

        public string ModelId { get; }

        public Type ModelType { get; }

        public HubEntityOperation OperationType { get; }

        public string SourceModelId { get; }

        public Type SourceModelType { get; }

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

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.ModelId)
                ? $"{this.ModelType.Name} {this.OperationType}"
                : $"{this.ModelType.Name}({this.ModelId}) {this.OperationType}";
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
