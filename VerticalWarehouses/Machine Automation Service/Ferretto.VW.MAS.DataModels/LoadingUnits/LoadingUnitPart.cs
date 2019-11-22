using Newtonsoft.Json;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class LoadingUnitPart : DataModel
    {
        #region Properties

        /// <summary>
        /// The loading unit that owns the LoadingUnitPart.
        /// </summary>
        [JsonIgnore]
        public LoadingUnit LoadingUnit { get; set; }

        public int LoadingUnitId { get; set; }

        public int PartId { get; set; }

        #endregion
    }
}
