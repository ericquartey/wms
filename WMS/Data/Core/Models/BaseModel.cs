using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BaseModel<TKey> : IModel<TKey>, IPolicyDescriptor
    {
        #region Fields

        private readonly ISet<IPolicy> policies = new HashSet<IPolicy>();

        #endregion

        #region Constructors

        protected BaseModel()
        {
        }

        #endregion

        #region Properties

        public TKey Id { get; set; }

        public IEnumerable<IPolicy> Policies => this.policies;

        #endregion

        #region Methods

        public void AddPolicy(IPolicy policy)
        {
            this.policies.Add(policy);
        }

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
