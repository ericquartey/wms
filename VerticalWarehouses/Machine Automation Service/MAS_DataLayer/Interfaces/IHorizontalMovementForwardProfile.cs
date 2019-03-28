using System.Threading.Tasks;

namespace Ferretto.VW.MAS_DataLayer.Interfaces
{
    public interface IHorizontalMovementForwardProfile
    {
        #region Properties

        Task<decimal> InitialSpeed { get; }

        Task<decimal> Step1AccDec { get; }

        Task<decimal> Step1Position { get; }

        Task<decimal> Step1Speed { get; }

        Task<decimal> Step2AccDec { get; }

        Task<decimal> Step2Position { get; }

        Task<decimal> Step2Speed { get; }

        Task<decimal> Step3AccDec { get; }

        Task<decimal> Step3Position { get; }

        Task<decimal> Step3Speed { get; }

        Task<decimal> Step4AccDec { get; }

        Task<decimal> Step4Position { get; }

        Task<decimal> Step4Speed { get; }

        Task<int> TotalSteps { get; }

        #endregion
    }
}
