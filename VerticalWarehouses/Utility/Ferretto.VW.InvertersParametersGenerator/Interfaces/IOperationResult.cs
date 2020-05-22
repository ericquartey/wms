using System.Windows.Input;

namespace Ferretto.VW.InvertersParametersGenerator.ViewModels
{
    public interface IOperationResult
    {
        #region Properties

        bool CanNext { get; }

        bool CanPrevious { get; }

        #endregion

        #region Methods

        bool Next();

        void Previous();

        #endregion
    }
}
