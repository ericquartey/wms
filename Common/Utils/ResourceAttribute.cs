using System;

namespace Ferretto.Common.Utils
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ResourceAttribute : Attribute
    {
        #region Constructors

        public ResourceAttribute(string resourceName, bool primary = true)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentException("Argument cannot be null or whitespace.", nameof(resourceName));
            }

            this.Primary = primary;
            this.ResourceName = resourceName;
        }

        #endregion

        #region Properties

        public bool Primary { get; }

        public string ResourceName { get; }

        #endregion
    }
}
