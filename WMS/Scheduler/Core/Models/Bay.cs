namespace Ferretto.WMS.Scheduler.Core
{
    public class Bay : Model
    {
        #region Properties

        public int? LoadingUnitsBufferSize { get; set; }

        public int LoadingUnitsBufferUsage { get; internal set; }

        #endregion

        // TODO: should LoadingUnitsBufferSize this be a non-nullable?
    }
}
