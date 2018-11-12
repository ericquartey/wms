using System;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public abstract class BusinessObject : BindableBase, IBusinessObject
    {
        #region Constructors

        public BusinessObject()
        { }

        #endregion Constructors

        #region Properties

        public int Id { get; set; }

        #endregion Properties

        #region Methods

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

        #endregion Methods
    }
}
