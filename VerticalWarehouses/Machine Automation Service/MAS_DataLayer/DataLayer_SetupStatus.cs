using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ISetupStatus
    {
        #region Properties

        public bool Bay1ControlDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Bay1ControlDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Bay1ControlDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Bay2ControlDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Bay2ControlDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Bay2ControlDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Bay3ControlDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Bay3ControlDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Bay3ControlDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool BeltBurnishingDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.BeltBurnishingDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.BeltBurnishingDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool CellsControlDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.CellsControlDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.CellsControlDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool DrawersLoadedDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.DrawersLoadedDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.DrawersLoadedDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool FirstDrawerLoadDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.FirstDrawerLoadDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.FirstDrawerLoadDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool HorizontalHomingDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.HorizontalHomingDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.HorizontalHomingDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Laser1Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Laser1Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Laser1Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Laser2Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Laser2Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Laser2Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Laser3Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Laser3Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Laser3Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool MachineDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool PanelsControlDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.PanelsControlDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.PanelsControlDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Shape1Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Shape1Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Shape1Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Shape2Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Shape2Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Shape2Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Shape3Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Shape3Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Shape3Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Shutter1Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Shutter1Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Shutter1Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Shutter2Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Shutter2Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Shutter2Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Shutter3Done
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Shutter3Done, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Shutter3Done, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool Undefined
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.Undefined, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.Undefined, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool VerticalHomingDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool VerticalOffsetDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool VerticalResolutionDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        public bool WheightMeasurementDone
        {
            get => this.GetBoolConfigurationValue((long)SetupStatus.WheightMeasurementDone, (long)ConfigurationCategory.SetupStatus);
            set => this.SetBoolConfigurationValue((long)SetupStatus.WheightMeasurementDone, (long)ConfigurationCategory.SetupStatus, value);
        }

        #endregion
    }
}
