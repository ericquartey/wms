using System;

namespace Ferretto.Common.Utils
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class ResourceAttribute : Attribute
    {
        private const string ArgumentCannotBeNullOrWhitespace = "Argument cannot be null or whitespace.";

        #region Constructors

        public ResourceAttribute(string resourceName, bool primary = true)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentException(ArgumentCannotBeNullOrWhitespace, nameof(resourceName));
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
