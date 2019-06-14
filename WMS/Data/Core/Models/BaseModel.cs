using System.Collections.Generic;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public class BaseModel<TKey> : IModel<TKey>, IPolicyDescriptor<Policy>
    {
        #region Fields

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

        #endregion
    }
}
