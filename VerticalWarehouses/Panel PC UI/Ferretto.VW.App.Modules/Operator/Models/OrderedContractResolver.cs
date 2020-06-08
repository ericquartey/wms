using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ferretto.VW.App.Modules.Operator.Models
{
    public class OrderedContractResolver : DefaultContractResolver
    {
        #region Methods

        protected override System.Collections.Generic.IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
        }

        #endregion
    }
}
