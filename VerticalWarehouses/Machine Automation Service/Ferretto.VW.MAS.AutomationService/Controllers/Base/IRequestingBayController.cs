using Ferretto.VW.CommonUtils.Messages.Enumerations;


namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public interface IRequestingBayController
    {
        #region Properties

        BayNumber BayNumber { get; set; }

        #endregion
    }
}
