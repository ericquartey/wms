using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IBayPositionControlDataLayer
    {
        #region Properties

        Task<decimal> StepValueBP { get; }

        #endregion
    }
}
