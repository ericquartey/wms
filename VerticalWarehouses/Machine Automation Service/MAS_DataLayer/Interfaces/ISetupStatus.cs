namespace Ferretto.VW.MAS.DataLayer.Interfaces
{
    public interface ISetupStatus
    {
        #region Properties

        bool Bay1ControlDone { get; }

        bool Bay2ControlDone { get; }

        bool Bay3ControlDone { get; }

        bool BeltBurnishingDone { get; }

        bool CellsControlDone { get; }

        bool DrawersLoadedDone { get; }

        bool FirstDrawerLoadDone { get; }

        bool HorizontalHomingDone { get; }

        bool Laser1Done { get; }

        bool Laser2Done { get; }

        bool Laser3Done { get; }

        bool MachineDone { get; }

        bool PanelsControlDone { get; }

        bool Shape1Done { get; }

        bool Shape2Done { get; }

        bool Shape3Done { get; }

        bool Shutter1Done { get; }

        bool Shutter2Done { get; }

        bool Shutter3Done { get; }

        bool VerticalHomingDone { get; }

        bool VerticalOffsetDone { get; }

        bool VerticalResolutionDone { get; }

        bool WeightMeasurementDone { get; }

        #endregion
    }
}
