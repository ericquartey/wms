namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface ISetupStatus
    {
        #region Properties

        bool Bay1ControlDone { get; set; }

        bool Bay2ControlDone { get; set; }

        bool Bay3ControlDone { get; set; }

        bool BeltBurnishingDone { get; set; }

        bool CellsControlDone { get; set; }

        bool DrawersLoadedDone { get; set; }

        bool FirstDrawerLoadDone { get; set; }

        bool HorizontalHomingDone { get; set; }

        bool Laser1Done { get; set; }

        bool Laser2Done { get; set; }

        bool Laser3Done { get; set; }

        bool MachineDone { get; set; }

        bool PanelsControlDone { get; set; }

        bool Shape1Done { get; set; }

        bool Shape2Done { get; set; }

        bool Shape3Done { get; set; }

        bool Shutter1Done { get; set; }

        bool Shutter2Done { get; set; }

        bool Shutter3Done { get; set; }

        bool VerticalHomingDone { get; set; }

        bool VerticalOffsetDone { get; set; }

        bool VerticalResolutionDone { get; set; }

        bool WheightMeasurementDone { get; set; }

        #endregion
    }
}
