using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.BLL.Interfaces.Models;
using Ferretto.Common.Utils;
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

        public override string this[string columnName] => !this.IsValidationEnabled ? null : this.GetErrorMessageIfRequired(columnName);

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public string GetErrorMessageForInvalid(string propertyName)
        {
            var localizedFieldName = PropertyMetadata.LocalizeFieldName(
                this.GetType(),
                propertyName);

            return string.Format(Common.Resources.Errors.PropertyValueIsInvalid, localizedFieldName);
        }

        public string GetErrorMessageIfRequired(string propertyName)
        {
            var type = this.GetType();
            var propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return null;
            }

            var isRequired = propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute));
            if (!isRequired)
            {
                return null;
            }

            if (propertyInfo.HasEmptyValue(this))
            {
                var localizedFieldName = PropertyMetadata.LocalizeFieldName(type, propertyName);
                return string.Format(Common.Resources.Errors.PropertyIsRequired, localizedFieldName);
            }

            return null;
        }

        protected string GetErrorMessageIfNegative(double? value, string propertyName)
        {
            if (value.HasValue && value.Value < 0)
            {
                var localizedFieldName = PropertyMetadata.LocalizeFieldName(
                    this.GetType(),
                    propertyName);

                return string.Format(Common.Resources.Errors.PropertyMustBePositive, localizedFieldName);
            }

            return null;
        }

        protected string GetErrorMessageIfNegativeOrZero(double? value, string propertyName)
        {
            if (value.HasValue && value.Value <= 0)
            {
                var localizedFieldName = PropertyMetadata.LocalizeFieldName(
                    this.GetType(),
                    propertyName);

                return string.Format(Common.Resources.Errors.PropertyMustBeStriclyPositive, localizedFieldName);
            }

            return null;
        }

        protected string GetErrorMessageIfZeroOrNull(int? value, string propertyName)
        {
            if (!value.HasValue || value.Value == 0)
            {
                var localizedFieldName = PropertyMetadata.LocalizeFieldName(
                    this.GetType(),
                    propertyName);

                return string.Format(Common.Resources.Errors.PropertyMustHaveValue, localizedFieldName);
            }

            return null;
        }

        protected string GetErrorMessageIfNullOrEmpty(string value, string propertyName)
        {
            if (string.IsNullOrEmpty(value))
            {
                var localizedFieldName = PropertyMetadata.LocalizeFieldName(
                    this.GetType(),
                    propertyName);

                return string.Format(Common.Resources.Errors.PropertyMustHaveValue, localizedFieldName);
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
