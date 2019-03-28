using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IBeltBurnishing
    {
        #region Properties

        Task<int> CycleQuantity { get; }

        #endregion
    }
}
