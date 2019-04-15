using System;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Scheduler.Core.Models
{
    public class Model : IModel<int>
    {
        #region Fields

        private const string ParameterMustBePositive = "Parameter must be positive.";

        private const string ParameterMustBeStrictlyPositive = "Parameter must be strictly positive.";

        #endregion

        #region Constructors

        protected Model()
        {
        }

        #endregion

        #region Properties

        public int Id { get; set; }

        #endregion

        #region Methods

        protected static int? CheckIfPositive(int? value)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBePositive);
            }

            return value;
        }

        protected static double? CheckIfPositive(double? value)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBePositive);
            }

            return value;
        }

        protected static double CheckIfPositive(double value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBePositive);
            }

            return value;
        }

        protected static int CheckIfPositive(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBePositive);
            }

            return value;
        }

        protected static int CheckIfStrictlyPositive(int value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBeStrictlyPositive);
            }

            return value;
        }

        protected static double? CheckIfStrictlyPositive(double? value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBeStrictlyPositive);
            }

            return value;
        }

        protected static int? CheckIfStrictlyPositive(int? value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBeStrictlyPositive);
            }

            return value;
        }

        protected static double CheckIfStrictlyPositive(double value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), ParameterMustBeStrictlyPositive);
            }

            return value;
        }

        #endregion
    }
}
