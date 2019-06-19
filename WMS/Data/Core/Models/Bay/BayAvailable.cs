namespace Ferretto.WMS.Data.Core.Models
{
    public class BayAvailable : BaseModel<int>
    {
        #region Properties

        [Positive]
        public int? LoadingUnitsBufferSize { get; set; }

        [PositiveOrZero]
        public int LoadingUnitsBufferUsage { get; internal set; }

        #endregion

        // TODO: should LoadingUnitsBufferSize this be a non-nullable?
    }
}
