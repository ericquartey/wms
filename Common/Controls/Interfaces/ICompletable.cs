using System;

namespace Ferretto.Common.Controls
{
    public interface ICompletable
    {
        #region Events

        event EventHandler<OperationEventArgs> OperationComplete;

        #endregion
    }
}
