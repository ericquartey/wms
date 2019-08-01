using System;

namespace Ferretto.VW.MAS.DataModels.Errors
{
    public class ErrorDescriptionAttribute : Attribute
    {
        #region Fields

        public readonly string propertyName;

        public readonly Type reasonResourceType;

        private readonly Type descriptionResourceType;

        #endregion

        #region Constructors

        public ErrorDescriptionAttribute(Type descriptionResourceType, Type reasonResourceType, string propertyName, int severity = 0)
        {
            if (descriptionResourceType == null)
            {
                throw new ArgumentNullException(nameof(descriptionResourceType));
            }

            if (reasonResourceType == null)
            {
                throw new ArgumentNullException(nameof(reasonResourceType));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            this.descriptionResourceType = descriptionResourceType;
            this.reasonResourceType = reasonResourceType;
            this.propertyName = propertyName;
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

        public string Reason { get; }

        public int Severity { get; }

        #endregion
    }
}
