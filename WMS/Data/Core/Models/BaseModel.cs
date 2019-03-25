using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BaseModel<TKey> : IModel<TKey>, IPolicyDescriptor<Policy>
    {
        #region Fields

        private const string parameterMustBePositiveMessage = "Parameter must be positive.";

        private const string parameterMustBeStrictlyPositiveMessage = "Parameter must be strictly positive.";

        private readonly ISet<Policy> policies = new HashSet<Policy>();

        #endregion

        #region Constructors

        protected BaseModel()
        {
        }

        #endregion

        #region Properties

        public TKey Id { get; set; }

        public IEnumerable<Policy> Policies => this.policies;

        #endregion

        #region Methods

        public void AddPolicy(Policy policy)
        {
            this.policies.Add(policy);
        }

        protected static int? CheckIfPositive(int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBePositiveMessage);
            }

            return value;
        }

        protected static int CheckIfPositive(int value, [CallerMemberName] string propertyName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBePositiveMessage);
            }

            return value;
        }

        protected static double? CheckIfPositive(double? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBePositiveMessage);
            }

            return value;
        }

        protected static double CheckIfPositive(double value, [CallerMemberName] string propertyName = null)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBePositiveMessage);
            }

            return value;
        }

        protected static int? CheckIfStrictlyPositive(int? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBeStrictlyPositiveMessage);
            }

            return value;
        }

        protected static int CheckIfStrictlyPositive(int value, [CallerMemberName] string propertyName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBeStrictlyPositiveMessage);
            }

            return value;
        }

        protected static double? CheckIfStrictlyPositive(double? value, [CallerMemberName] string propertyName = null)
        {
            if (value.HasValue && value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBeStrictlyPositiveMessage);
            }

            return value;
        }

        protected static double CheckIfStrictlyPositive(double value, [CallerMemberName] string propertyName = null)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), parameterMustBeStrictlyPositiveMessage);
            }

            return value;
        }

        #endregion
    }
}
