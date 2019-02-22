using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Ferretto.Common.BusinessModels
{
    public class ChangeDetector<T> : IDisposable
        where T : class, ICloneable, INotifyPropertyChanged
    {
        #region Fields

        private readonly ISet<string> modifiedProperties = new HashSet<string>();

        private readonly ISet<string> requiredProperties = new HashSet<string>();

        private T instance;

        private bool isModified;

        private bool isRequiredValid;

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

        public bool IsRequiredValid
        {
            get => this.isRequiredValid;
            private set
            {
                if (this.isRequiredValid != value)
                {
                    this.isRequiredValid = value;
                    this.ModifiedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

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
            if (this.instance != null && !this.instance.Equals(newInstance))
            {
                this.instance.PropertyChanged -= this.Instance_PropertyChanged;
            }

            this.instance = newInstance;
            this.modifiedProperties.Clear();
            this.requiredProperties.Clear();
            this.IsModified = false;

            this.totalRequired = this.instance.GetType().GetProperties()
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute))).Count();

            if (newInstance != null)
            {
                newInstance.PropertyChanged += this.Instance_PropertyChanged;
                this.snapshot = newInstance.Clone() as T;
            }
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.instance == null || this.snapshot == null)
            {
                return;
            }

            var propertyInfo = this.instance.GetType().GetProperty(e.PropertyName);
            var newValue = propertyInfo.GetValue(sender);
            var snapshotValue = propertyInfo.GetValue(this.snapshot);

            var isRequired = propertyInfo.CustomAttributes.Any(a => a.AttributeType == typeof(RequiredAttribute));

            if (newValue != snapshotValue && newValue?.Equals(snapshotValue) == false)
            {
                if (this.modifiedProperties.Contains(e.PropertyName) == false)
                {
                    this.modifiedProperties.Add(e.PropertyName);
                    if (isRequired)
                    {
                        this.requiredProperties.Add(e.PropertyName);
                    }
                    NLog.LogManager.GetCurrentClassLogger().Trace($"Property '{this.instance.GetType().Name}.{e.PropertyName}' was modified.");
                }
            }
            else if (this.modifiedProperties.Contains(e.PropertyName))
            {
                this.modifiedProperties.Remove(e.PropertyName);
                if (isRequired)
                {
                    this.requiredProperties.Remove(e.PropertyName);
                }
                NLog.LogManager.GetCurrentClassLogger().Trace($"Property '{this.instance.GetType().Name}.{e.PropertyName}' was reset to initial value.");
            }

            this.IsModified = this.modifiedProperties.Count > 0;
            this.IsRequiredValid = this.requiredProperties.Count == this.totalRequired;
        }

        #endregion
    }
}
