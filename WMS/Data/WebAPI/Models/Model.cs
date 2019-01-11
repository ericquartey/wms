using System;
using System.Runtime.CompilerServices;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public abstract class Model<TKey>
    {
        #region Properties

        public TKey Id { get; set; }

        #endregion Properties

        #region Methods

        protected int? CheckIfPositive(int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be positive.");
            }

            return value;
        }

        protected int CheckIfPositive(int value, [CallerMemberName] string propertyName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be positive.");
            }

            return value;
        }

        protected int? CheckIfStrictlyPositive(int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
            }

            return value;
        }

        protected int CheckIfStrictlyPositive(int value, [CallerMemberName] string propertyName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
            }

            return value;
        }

        #endregion Methods
    }
}
