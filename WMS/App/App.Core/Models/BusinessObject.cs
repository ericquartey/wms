using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.App.Core.Models
{
    public class BusinessObject : BindableBase, ICloneable, IModel<int>, IPolicyDescriptor<Policy>
    {
        #region Constructors

        protected BusinessObject()
        {
        }

        #endregion

        #region Properties

        public int Id { get; set; }

        public IEnumerable<Policy> Policies { get; set; }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsRequiredValid(columnName))
                {
                    return string.Format(Common.Resources.Errors.PropertyIsRequired, columnName);
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

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);

            if (args?.PropertyName != nameof(this.Error))
            {
                this.RaisePropertyChanged(nameof(this.Error));
            }
        }

        #endregion
    }
}
