using System;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class ErrorDescriptionAttribute : Attribute
    {
        #region Constructors

        public ErrorDescriptionAttribute(Type descriptionResourceType, Type reasonResourceType, string propertyName, int severity = 0)
        {
            this.DescriptionResourceType = descriptionResourceType ?? throw new ArgumentNullException(nameof(descriptionResourceType));
            this.ReasonResourceType = reasonResourceType ?? throw new ArgumentNullException(nameof(reasonResourceType));
            this.PropertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));

            this.Severity = severity;

            this.Description = descriptionResourceType
                .GetProperty(
                    propertyName,
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?
                .GetValue(null) as string;

            this.Reason = reasonResourceType
                .GetProperty(
                    propertyName,
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)?
                .GetValue(null) as string;
        }

        #endregion

        #region Properties

        public string Description { get; }

        public Type DescriptionResourceType { get; private set; }

        public string PropertyName { get; private set; }

        public string Reason { get; }

        public Type ReasonResourceType { get; private set; }

        public int Severity { get; }

        #endregion
    }
}
