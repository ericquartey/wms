using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces.Base;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class BusinessObject : BindableBase, ICloneable, IModel<int>
    {
        #region Constructors

        protected BusinessObject()
        {
        }

        #endregion

        #region Properties

        public int Id { get; set; }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsRequiredValid(columnName))
                {
                    return string.Format(Resources.Errors.PropertyIsRequired, columnName);
                }

                return string.Empty;
            }
        }

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool HasEmptyValue(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return true;
            }

            var propertyType = propertyInfo.PropertyType;
            var propertyValue = propertyInfo.GetValue(this);
            if (propertyType.IsEnum)
            {
                return propertyValue == null || (int)propertyValue == 0;
            }

            if (propertyType == typeof(DateTime))
            {
                return propertyValue == null || (DateTime)propertyValue == DateTime.MinValue;
            }

            return propertyValue == null;
        }

        public bool IsRequiredValid(string columnName)
        {
            var propertyInfo = this.GetType().GetProperty(columnName);
            if (propertyInfo == null)
            {
                return true;
            }

            var isRequired = propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute));
            if (!isRequired)
            {
                return true;
            }

            return !this.HasEmptyValue(propertyInfo);
        }

        protected bool SetIfPositive(ref int? member, int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
            }

            return this.SetProperty(ref member, value, propertyName);
        }

        protected bool SetIfPositive(ref int member, int value, [CallerMemberName] string propertyName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
            }

            return this.SetProperty(ref member, value, propertyName);
        }

        protected bool SetIfStrictlyPositive(ref int? member, int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
            }

            return this.SetProperty(ref member, value, propertyName);
        }

        protected bool SetIfStrictlyPositive(ref int member, int value, [CallerMemberName] string propertyName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
            }

            return this.SetProperty(ref member, value, propertyName);
        }

        #endregion
    }
}
