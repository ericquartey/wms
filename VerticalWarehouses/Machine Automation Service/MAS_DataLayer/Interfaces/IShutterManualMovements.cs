using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IShutterManualMovements
    {
        #region Properties

        Task<decimal> Acceleration { get; }

        Task<decimal> Deceleration { get; }

        Task<decimal> FeedRate { get; }

        Task<decimal> MaxSpeed { get; }

        #endregion
    }
}
