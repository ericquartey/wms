using System.Threading.Tasks;

namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ISetupStatus
    {
        #region Properties

        Task<bool> Bay1ControlDone { get; }

        Task<bool> Bay2ControlDone { get; }

        Task<bool> Bay3ControlDone { get; }

        Task<bool> BeltBurnishingDone { get; }

        Task<bool> CellsControlDone { get; }

        Task<bool> DrawersLoadedDone { get; }

        Task<bool> FirstDrawerLoadDone { get; }

        Task<bool> HorizontalHomingDone { get; }

        Task<bool> Laser1Done { get; }

        Task<bool> Laser2Done { get; }

        Task<bool> Laser3Done { get; }

        Task<bool> MachineDone { get; }

        Task<bool> PanelsControlDone { get; }

        Task<bool> Shape1Done { get; }

        Task<bool> Shape2Done { get; }

        Task<bool> Shape3Done { get; }

        Task<bool> Shutter1Done { get; }

        Task<bool> Shutter2Done { get; }

        Task<bool> Shutter3Done { get; }

        Task<bool> VerticalHomingDone { get; }

        Task<bool> VerticalOffsetDone { get; }

        Task<bool> VerticalResolutionDone { get; }

        Task<bool> WeightMeasurementDone { get; }

        #endregion
    }
}
