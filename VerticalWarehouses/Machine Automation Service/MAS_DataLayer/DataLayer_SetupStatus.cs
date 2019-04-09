using System.Threading.Tasks;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;

namespace Ferretto.VW.MAS_DataLayer
{
    public partial class DataLayer : ISetupStatus
    {
        #region Properties

        public Task<bool> Bay1ControlDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Bay1ControlDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Bay2ControlDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Bay2ControlDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Bay3ControlDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Bay3ControlDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> BeltBurnishingDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.BeltBurnishingDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> CellsControlDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.CellsControlDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> DrawersLoadedDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.DrawersLoadedDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> FirstDrawerLoadDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.FirstDrawerLoadDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> HorizontalHomingDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.HorizontalHomingDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Laser1Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Laser1Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Laser2Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Laser2Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Laser3Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Laser3Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> MachineDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> PanelsControlDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.PanelsControlDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Shape1Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shape1Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Shape2Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shape2Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Shape3Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shape3Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Shutter1Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shutter1Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Shutter2Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shutter2Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> Shutter3Done
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.Shutter3Done, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> VerticalHomingDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalHomingDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> VerticalOffsetDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> VerticalResolutionDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.VerticalResolutionDone, (long)ConfigurationCategory.SetupStatus);
        }

        public Task<bool> WeightMeasurementDone
        {
            get => this.GetBoolConfigurationValueAsync((long)SetupStatus.WheightMeasurementDone, (long)ConfigurationCategory.SetupStatus);
        }

        #endregion
    }
}
