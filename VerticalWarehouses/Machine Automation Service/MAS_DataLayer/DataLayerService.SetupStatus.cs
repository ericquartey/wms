using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.DataLayer
{
    public partial class DataLayerService : ISetupStatus
    {
        #region Properties

        public bool Bay1ControlDone => this.GetBoolConfigurationValue((long)SetupStatus.Bay1ControlDone, (long)ConfigurationCategory.SetupStatus);

        public bool Bay2ControlDone => this.GetBoolConfigurationValue((long)SetupStatus.Bay2ControlDone, (long)ConfigurationCategory.SetupStatus);

        public bool Bay3ControlDone => this.GetBoolConfigurationValue((long)SetupStatus.Bay3ControlDone, (long)ConfigurationCategory.SetupStatus);

        public bool BeltBurnishingDone => this.GetBoolConfigurationValue((long)SetupStatus.BeltBurnishingDone, (long)ConfigurationCategory.SetupStatus);

        public bool CellsControlDone => this.GetBoolConfigurationValue((long)SetupStatus.CellsControlDone, (long)ConfigurationCategory.SetupStatus);

        public bool DrawersLoadedDone => this.GetBoolConfigurationValue((long)SetupStatus.DrawersLoadedDone, (long)ConfigurationCategory.SetupStatus);

        public bool FirstDrawerLoadDone => this.GetBoolConfigurationValue((long)SetupStatus.FirstDrawerLoadDone, (long)ConfigurationCategory.SetupStatus);

        public bool HorizontalHomingDone => this.GetBoolConfigurationValue((long)SetupStatus.HorizontalHomingDone, (long)ConfigurationCategory.SetupStatus);

        public bool Laser1Done => this.GetBoolConfigurationValue((long)SetupStatus.Laser1Done, (long)ConfigurationCategory.SetupStatus);

        public bool Laser2Done => this.GetBoolConfigurationValue((long)SetupStatus.Laser2Done, (long)ConfigurationCategory.SetupStatus);

        public bool Laser3Done => this.GetBoolConfigurationValue((long)SetupStatus.Laser3Done, (long)ConfigurationCategory.SetupStatus);

        public bool MachineDone => this.GetBoolConfigurationValue((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);

        public bool PanelsControlDone => this.GetBoolConfigurationValue((long)SetupStatus.PanelsControlDone, (long)ConfigurationCategory.SetupStatus);

        public bool Shape1Done => this.GetBoolConfigurationValue((long)SetupStatus.Shape1Done, (long)ConfigurationCategory.SetupStatus);

        public bool Shape2Done => this.GetBoolConfigurationValue((long)SetupStatus.Shape2Done, (long)ConfigurationCategory.SetupStatus);

        public bool Shape3Done => this.GetBoolConfigurationValue((long)SetupStatus.Shape3Done, (long)ConfigurationCategory.SetupStatus);

        public bool Shutter1Done => this.GetBoolConfigurationValue((long)SetupStatus.Shutter1Done, (long)ConfigurationCategory.SetupStatus);

        public bool Shutter2Done => this.GetBoolConfigurationValue((long)SetupStatus.Shutter2Done, (long)ConfigurationCategory.SetupStatus);

        public bool Shutter3Done => this.GetBoolConfigurationValue((long)SetupStatus.Shutter3Done, (long)ConfigurationCategory.SetupStatus);

        public bool VerticalHomingDone => this.GetBoolConfigurationValue((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus);

        public bool VerticalOffsetDone => this.GetBoolConfigurationValue((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus);

        public bool VerticalResolutionDone => this.GetBoolConfigurationValue((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus);

        public bool WeightMeasurementDone => this.GetBoolConfigurationValue((long)SetupStatus.WeightMeasurementDone, (long)ConfigurationCategory.SetupStatus);

        #endregion
    }
}
