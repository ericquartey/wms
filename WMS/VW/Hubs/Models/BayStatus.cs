namespace Ferretto.VW.MachineAutomationService.Hubs
{
    public class BayStatus
    {
        #region Properties

        public int BayId { get; set; }

        public int? LoadingUnitId { get; set; }

        public int? LoggedUserId { get; set; }

        #endregion
    }
}
