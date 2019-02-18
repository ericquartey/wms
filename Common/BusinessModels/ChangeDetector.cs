using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ferretto.Common.BusinessModels
{
    public class ChangeDetector<T> : IDisposable
        where T : class, ICloneable, INotifyPropertyChanged
    {
        #region Fields

        private readonly ISet<string> modifiedProperties = new HashSet<string>();
        private T instance;
        private bool isModified;
        private T snapshot;

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

        #endregion

        #region Methods

        public void Dispose()
        {
            this.instance.PropertyChanged -= this.Instance_PropertyChanged;
        }

        public void TakeSnapshot(T newInstance)
        {
            if (!this.instance.Equals(newInstance))
            {
                if (this.instance != null)
                {
                    this.instance.PropertyChanged -= this.Instance_PropertyChanged;
                }

                this.instance = newInstance;
                this.modifiedProperties.Clear();
                this.IsModified = false;

                if (newInstance != null)
                {
                    newInstance.PropertyChanged += this.Instance_PropertyChanged;
                    this.snapshot = newInstance.Clone() as T;
                }
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
            if (newValue != snapshotValue && newValue?.Equals(snapshotValue) == false)
            {
                if (this.modifiedProperties.Contains(e.PropertyName) == false)
                {
                    this.modifiedProperties.Add(e.PropertyName);
                    NLog.LogManager.GetCurrentClassLogger().Trace($"Property '{this.instance.GetType().Name}.{e.PropertyName}' was modified.");
                }
            }
            else if (this.modifiedProperties.Contains(e.PropertyName))
            {
                this.modifiedProperties.Remove(e.PropertyName);
                NLog.LogManager.GetCurrentClassLogger().Trace($"Property '{this.instance.GetType().Name}.{e.PropertyName}' was reset to initial value.");
            }

            this.IsModified = this.modifiedProperties.Count > 0;
        }

        #endregion
    }
}
