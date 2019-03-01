using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Ferretto.Common.BusinessModels
{
    public class ChangeDetector<T> : IDisposable
        where T : class, ICloneable, INotifyPropertyChanged
    {
        #region Fields

        private readonly ISet<string> modifiedProperties = new HashSet<string>();

        private readonly ISet<string> validRequiredProperties = new HashSet<string>();

        private T instance;

        private bool isModified;

        private T snapshot;

        private int totalRequired;

        #endregion

        #region Events

        public event EventHandler ModifiedChanged;

        #endregion

        #region Properties

        public bool IsModified
        {
            get => this.isModified;
            private set
            {
                if (this.isModified != value)
                {
                    this.isModified = value;
                    this.ModifiedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool IsRequiredValid { get; private set; }

        #endregion

        #region Methods

        public void Dispose()
        {
            if (this.instance != null)
            {
                this.instance.PropertyChanged -= this.Instance_PropertyChanged;
            }
        }

        public void TakeSnapshot(T newInstance)
        {
            if (this.instance != null)
            {
                this.instance.PropertyChanged -= this.Instance_PropertyChanged;
            }

            this.instance = newInstance;
            this.modifiedProperties.Clear();
            this.validRequiredProperties.Clear();
            this.IsModified = false;

            var instanceValidRequiredProperties = this.instance.GetType()
                           .GetProperties().Where(
                               p => p.CustomAttributes.Any(
                                        a => a.AttributeType == typeof(RequiredAttribute))
                                    && !HasEmptyValue(p, this.instance)).ToArray();

            foreach (var validRequiredProperty in instanceValidRequiredProperties)
            {
                this.validRequiredProperties.Add(validRequiredProperty.Name);
            }

            if (newInstance == null)
            {
                return;
            }

          
                this.totalRequired = this.instance.GetType().GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute))).Count();

                newInstance.PropertyChanged += this.Instance_PropertyChanged;
                this.snapshot = newInstance.Clone() as T;
            
        }

        private static bool HasEmptyValue(PropertyInfo propertyInfo, T model)
        {
            if (propertyInfo == null)
            {
                return true;
            }

            var propertyType = propertyInfo.PropertyType;
            var propertyValue = propertyInfo.GetValue(model);
            if (propertyType.IsEnum)
            {
                return propertyValue == null || (int)propertyValue == 0;
            }

            if (propertyType == typeof(DateTime))
            {
                return propertyValue == null || (DateTime)propertyValue == DateTime.MinValue;
            }

            return propertyInfo.GetValue(model) == null;
        }

        private static bool IsValueChanged(PropertyInfo propertyInfo, T originalModel, T actualModel)
        {
            var actualValue = propertyInfo.GetValue(actualModel);
            var originalValue = propertyInfo.GetValue(originalModel);
            return actualValue != originalValue && (actualValue == null || actualValue.Equals(originalValue) == false);
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.instance == null || this.snapshot == null)
            {
                return;
            }

            var propertyName = e.PropertyName;
            var propertyInfo = this.instance.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return;
            }

            var isPresent = this.modifiedProperties.Contains(propertyName);
            if (IsValueChanged(propertyInfo, this.snapshot, this.instance))
            {
                if (!isPresent)
                {
                    this.modifiedProperties.Add(e.PropertyName);
                }

                this.UpdateValidRequiredProperties(propertyInfo, this.instance);
            }
            else if (isPresent)
            {
                this.modifiedProperties.Remove(e.PropertyName);
            }

            this.IsModified = this.modifiedProperties.Count > 0;
            this.IsRequiredValid = this.validRequiredProperties.Count == this.totalRequired;
        }

        private void UpdateValidRequiredProperties(PropertyInfo propertyInfo, T model)
        {
            var isRequired = propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute));
            if (!isRequired)
            {
                return;
            }

            var propertyName = propertyInfo.Name;
            var isPresent = this.validRequiredProperties.Contains(propertyName);
            if (HasEmptyValue(propertyInfo, model))
            {
                if (isPresent)
                {
                    this.validRequiredProperties.Remove(propertyName);
                }
            }
            else if (!isPresent)
            {
                this.validRequiredProperties.Add(propertyName);
            }
        }

        #endregion
    }
}
