using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface ICapacityCompartment : IModel<int>
    {
        int Stock { get; }
    }
}
