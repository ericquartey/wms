namespace Ferretto.Common.BusinessModels
{
    public abstract class SchedulerRequest : BusinessObject
    {
        #region Properties

        public int? AreaId { get; set; }
        public int? BayId { get; set; }
        public System.DateTime CreationDate { get; set; }
        public string OperationType { get; set; }

        #endregion Properties
    }
}
