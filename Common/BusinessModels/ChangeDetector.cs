using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Ferretto.Common.BusinessModels
{
    public class ChangeDetector<T> : IDisposable where T : class, ICloneable, INotifyPropertyChanged
    {
        #region Fields

        private readonly ISet<string> modifiedProperties = new HashSet<string>();
        private T instance;
        private bool isModified;
        private T snapshot;

        #endregion Fields

        #region Events

        public event EventHandler ModifiedChanged;

        #endregion Events

        #region Properties

        public bool IsModified
        {
            get => this.isModified;
            private set
            {
                if (this.isModified != value)
                {
                    this.isModified = value;
                    this.ModifiedChanged?.Invoke(this, null);
                }
            }
        }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            this.instance.PropertyChanged -= this.Instance_PropertyChanged;
        }

        public void TakeSnapshot(T instance)
        {
            if (instance != null && this.instance != instance)
            {
                if (this.instance != null)
                {
                    this.instance.PropertyChanged -= this.Instance_PropertyChanged;
                }

                this.instance = instance;
                this.instance.PropertyChanged += this.Instance_PropertyChanged;

                this.snapshot = this.instance.Clone() as T;
                this.modifiedProperties.Clear();
                this.IsModified = false;
            }
        }

        private void Instance_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (this.instance == null || this.snapshot == null)
            {
                return;
            }

            var propertyInfo = this.instance.GetType().GetProperty(e.PropertyName);
            var newValue = propertyInfo.GetValue(sender);
            var snapshotValue = propertyInfo.GetValue(this.snapshot);
            if (newValue?.Equals(snapshotValue) == false)
            {
                if (this.modifiedProperties.Contains(e.PropertyName) == false)
                {
                    this.modifiedProperties.Add(e.PropertyName);
                }
            }
            else if (this.modifiedProperties.Contains(e.PropertyName))
            {
                this.modifiedProperties.Remove(e.PropertyName);
            }

            this.IsModified = this.modifiedProperties.Count > 0;
        }

        #endregion Methods
    }
}
