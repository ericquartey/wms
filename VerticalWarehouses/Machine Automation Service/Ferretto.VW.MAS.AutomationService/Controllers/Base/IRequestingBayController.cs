using Ferretto.VW.CommonUtils.Messages.Enumerations;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public interface IRequestingBayController
    {
        #region Properties

        BayNumber BayNumber { get; set; }

        #endregion
    }
}
