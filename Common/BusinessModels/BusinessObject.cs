using System;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public abstract class BusinessObject : BindableBase, ICloneable, IBusinessObject
    {
        #region Properties

        public int Id { get; set; }

        #endregion

        #region Methods

        public object Clone()
        {
            return this.MemberwiseClone();
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
