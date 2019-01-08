namespace Ferretto.WMS.Scheduler.Core
{
    public class Bay : BusinessObject
    {
        #region Properties

        public int? LoadingUnitsBufferSize { get; set; }
        public int LoadingUnitsBufferUsage { get; internal set; }

        #endregion Properties

        // TODO: should LoadingUnitsBufferSize this be a non-nullable?
    }
}
