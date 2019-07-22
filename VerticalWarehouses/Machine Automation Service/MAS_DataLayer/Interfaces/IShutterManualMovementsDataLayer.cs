using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface IShutterManualMovementsDataLayer
    {
        #region Properties

        Task<decimal> Acceleration { get; }

        Task<decimal> Deceleration { get; }

        Task<decimal> FeedRate { get; }

        Task<decimal> MaxSpeed { get; }

        #endregion
    }
}
