using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public abstract class BusinessObject : BindableBase, ICloneable, IBusinessObject, IDisposable
    {
        #region Fields

        private bool isModified;
        private ISet<string> modifiedProperties;
        private BusinessObject snapshot;

        #endregion Fields

        #region Constructors

        public BusinessObject()
        {
            this.PropertyChanged += BusinessObject_PropertyChanged;
        }

        #endregion Constructors

        #region Properties

        public int Id { get; set; }

        public bool IsModified
        {
            get => this.isModified;
            private set => this.SetProperty(ref this.isModified, value);
        }

        #endregion Properties

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public void Dispose()
        {
            this.PropertyChanged -= BusinessObject_PropertyChanged;
        }

        public void TakeSnapshot()
        {
            this.snapshot = this.Clone() as BusinessObject;
            this.modifiedProperties = new HashSet<string>();
            this.IsModified = false;
        }

        protected bool SetIfPositive(ref int? member, int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue)
            {
                if (value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    return this.SetProperty(ref member, value, propertyName);
                }
            }

            return false;
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
            if (value.HasValue)
            {
                if (value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    return this.SetProperty(ref member, value, propertyName);
                }
            }

            return false;
        }

        protected bool SetIfStrictlyPositive(ref int member, int value, [CallerMemberName] string propertyName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
            }

            return this.SetProperty(ref member, value, propertyName);
        }

        private static void BusinessObject_PropertyChanged(Object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var bo = sender as BusinessObject;
            if (bo.snapshot == null || e.PropertyName == nameof(IsModified))
            {
                return;
            }

            var propertyInfo = bo.GetType().GetProperty(e.PropertyName);
            var newValue = propertyInfo.GetValue(sender);
            var snapshotValue = propertyInfo.GetValue(bo.snapshot);
            if (newValue.Equals(snapshotValue) == false)
            {
                if (bo.modifiedProperties.Contains(e.PropertyName) == false)
                {
                    bo.modifiedProperties.Add(e.PropertyName);
                }
            }
            else if (bo.modifiedProperties.Contains(e.PropertyName))
            {
                bo.modifiedProperties.Remove(e.PropertyName);
            }

            bo.IsModified = bo.modifiedProperties.Count > 0;
        }

        #endregion Methods
    }
}
