using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IPairedCompartment : IModel<int>
    {
        bool IsItemPairingFixed { get; }
    }
}
