using System;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.WMS.Data.Core
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class PositiveAttribute : ValidationAttribute
    {
        #region Methods

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                if (value is int intValue && intValue <= 0)
                {
                    return new ValidationResult(
                        string.Format(
                            Resources.Errors.ValueOfMustBeStrictlyPositive,
                            validationContext?.DisplayName));
                }

                if (value is double doubleValue && doubleValue <= 0)
                {
                    return new ValidationResult(
                        string.Format(
                            Resources.Errors.ValueOfMustBeStrictlyPositive,
                            validationContext?.DisplayName));
                }
            }

            return ValidationResult.Success;
        }

        #endregion
    }
}
