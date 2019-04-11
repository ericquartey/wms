using System;

namespace Ferretto.Common.Utils
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ResourceAttribute : Attribute
    {
        #region Constructors

        public ResourceAttribute(string resourceName)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentException("Argument cannot be null or whitespace.", nameof(resourceName));
            }

            this.ResourceName = resourceName;
        }

        #endregion

        #region Properties

        public string ResourceName { get; }

        #endregion
    }
}
