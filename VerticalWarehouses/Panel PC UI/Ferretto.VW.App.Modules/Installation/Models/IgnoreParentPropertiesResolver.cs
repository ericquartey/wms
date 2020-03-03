using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ferretto.VW.App.Modules.Installation.Models
{
    public class IgnoreParentPropertiesResolver : DefaultContractResolver
    {
        #region Fields

        private readonly bool ignoreBase = false;

        #endregion

        #region Constructors

        public IgnoreParentPropertiesResolver(bool ignoreBase)
        {
            this.ignoreBase = ignoreBase;
        }

        #endregion

        #region Methods

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var allProps = base.CreateProperties(type, memberSerialization);
            if (!this.ignoreBase)
            {
                return allProps;
            }

            var props = type.GetProperties(~BindingFlags.FlattenHierarchy | BindingFlags.SetProperty | BindingFlags.Public);

            return allProps.Where(p => props.Any(a => a.Name == p.PropertyName)).ToList();
        }

        #endregion
    }
}
