using System;
using Ferretto.Common.BLL.Interfaces;
using Ferretto.Common.Resources;
using Prism.Mvvm;

namespace Ferretto.Common.Modules.BLL.Models
{
    public class BusinessObject : BindableBase, IBusinessObject
    {
        #region Constructors

        protected BusinessObject()
        { }

        #endregion Constructors

        #region Methods

        protected bool SetIfPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    return this.SetProperty(ref member, value);
                }
            }

            return false;
        }

        protected bool SetIfPositive(ref int member, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
            }

            return this.SetProperty(ref member, value);
        }

        protected bool SetIfStrictlyPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    return this.SetProperty(ref member, value);
                }
            }

            return false;
        }

        protected bool SetIfStrictlyPositive(ref int member, int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBeStrictlyPositive);
            }

            return this.SetProperty(ref member, value);
        }

        #endregion Methods
    }
}
