using System;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BaseModel<TKey> : IModel<TKey>
    {
        #region Constructors

        protected BaseModel()
        {
        }

        #endregion

        #region Properties

        public TKey Id { get; set; }

        #endregion

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

        #endregion
    }
}
