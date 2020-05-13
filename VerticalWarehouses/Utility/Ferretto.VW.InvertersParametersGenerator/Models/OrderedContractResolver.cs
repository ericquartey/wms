using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Ferretto.VW.InvertersParametersGenerator.Models
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
