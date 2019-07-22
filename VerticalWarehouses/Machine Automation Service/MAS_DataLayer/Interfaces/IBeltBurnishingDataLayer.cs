using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IBeltBurnishingDataLayer
    {
        #region Properties

        Task<int> CycleQuantity { get; }

        #endregion
    }
}
