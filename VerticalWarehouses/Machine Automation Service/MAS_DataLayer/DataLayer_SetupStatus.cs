using System.Threading.Tasks;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ISetupStatus
    {
        #region Properties

        public Task<bool> Bay1ControlDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.Bay1ControlDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Bay2ControlDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.Bay2ControlDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Bay3ControlDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.Bay3ControlDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> BeltBurnishingDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.BeltBurnishingDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> CellsControlDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.CellsControlDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> DrawersLoadedDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.DrawersLoadedDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> FirstDrawerLoadDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.FirstDrawerLoadDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> HorizontalHomingDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.HorizontalHomingDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Laser1Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Laser1Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Laser2Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Laser2Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Laser3Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Laser3Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> MachineDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> PanelsControlDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.PanelsControlDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Shape1Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shape1Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Shape2Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shape2Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Shape3Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shape3Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Shutter1Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shutter1Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Shutter2Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shutter2Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> Shutter3Done => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shutter3Done, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> VerticalHomingDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> VerticalOffsetDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> VerticalResolutionDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus);

        public Task<bool> WeightMeasurementDone => this.GetBoolConfigurationValueAsync((long)SetupStatus.WeightMeasurementDone, (long)ConfigurationCategory.SetupStatus);

        #endregion
    }
}
