using System;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PositiveOrZeroAttribute : ValidationAttribute
    {
        #region Methods

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                if (value is int intValue && intValue < 0)
                {
                    return new ValidationResult($"Value of '{validationContext?.DisplayName}' must be positive.");
                }

                if (value is double doubleValue && doubleValue < 0)
                {
                    return new ValidationResult($"Value of '{validationContext?.DisplayName}' must be positive.");
                }
            }

            return ValidationResult.Success;
        }

        #endregion
    }
}
