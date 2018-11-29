using System;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.Scheduler.Core
{
    public abstract class BusinessObject
    {
        #region Properties

        public int Id { get; set; }

        #endregion Properties

        #region Methods

        protected static bool SetIfPositive(ref int? member, int? value)
        {
            if (value.HasValue)
            {
                if (value.Value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
                }

                if (!member.HasValue || member.Value != value.Value)
                {
                    member = value;
                    return true;
                }
            }

            return false;
        }

        protected static bool SetIfPositive(ref int member, int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), Errors.ParameterMustBePositive);
            }

            if (member != value)
            {
                member = value;
                return true;
            }

            return false;
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
                    member = value;
                    return true;
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

            if (member != value)
            {
                member = value;
                return true;
            }

            return false;
        }

        #endregion Methods
    }
}
