using System;
using System.Runtime.CompilerServices;

namespace Ferretto.WMS.Data.WebAPI.Models
{
    public class Model<TKey>
    {
        #region Constructors

        protected Model()
        {
        }

        #endregion Constructors

        #region Properties

        public TKey Id { get; set; }

        #endregion Properties

        #region Methods

        protected static int? CheckIfPositive(int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be positive.");
            }

            return value;
        }

        protected static int CheckIfPositive(int value, [CallerMemberName] string propertyName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be positive.");
            }

            return value;
        }

        protected static int? CheckIfStrictlyPositive(int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Parameter must be strictly positive.");
            }

            return value;
        }

        protected static int CheckIfStrictlyPositive(int value, [CallerMemberName] string propertyName = null)
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
