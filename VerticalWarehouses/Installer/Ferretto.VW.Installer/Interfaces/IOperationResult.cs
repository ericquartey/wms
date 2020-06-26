#nullable enable

namespace Ferretto.VW.Installer
{
    public interface IOperationResult
    {
        #region Properties

        bool IsSuccessful { get; }

        #endregion
    }
}
