using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils.Extensions;

namespace Ferretto.WMS.App.Core.Models
{
    public class BusinessObject : BindableBase, ICloneable, IModel<int>, IPolicyDescriptor<Policy>, IValidationEnable
    {
        #region Fields

        private bool isValidationEnabled;

        #endregion

        #region Constructors

        protected BusinessObject()
        {
        }

        #endregion

        #region Properties

        public override string Error =>
            string.Join(
                Environment.NewLine,
                this.GetType().GetProperties().Select(p => this[p.Name])
                    .Distinct()
                    .Where(s => !string.IsNullOrEmpty(s)));

        public int Id { get; set; }

        public bool IsValidationEnabled
        {
            get => this.isValidationEnabled;
            set
            {
                this.isValidationEnabled = value;
                this.RaisePropertyChanged(string.Empty);
            }
        }

        public IEnumerable<Policy> Policies { get; set; }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                if (!this.IsRequiredValid(columnName))
                {
                    return string.Format(Common.Resources.Errors.PropertyIsRequired, columnName);
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
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

            return !propertyInfo.HasEmptyValue(this);
        }

        protected static string GetErrorMessageIfNegative(double? value, string propertyName)
        {
            if (value.HasValue && value.Value < 0)
            {
                return string.Format(Common.Resources.Errors.PropertyMustBePositive, propertyName);
            }

            return null;
        }

        protected static string GetErrorMessageIfZeroOrNull(int? value, string propertyName)
        {
            if (!value.HasValue || value.Value == 0)
            {
                return string.Format(Common.Resources.Errors.PropertyMustHaveValue, propertyName);
            }

            return null;
        }

        protected static string GetErrorMessageIfNegativeOrZero(double? value, string propertyName)
        {
            if (value.HasValue && value.Value <= 0)
            {
                return string.Format(Common.Resources.Errors.PropertyMustBeStriclyPositive, propertyName);
            }

            return null;
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
