using System;

namespace Ferretto.WMS.App.Controls
{
    public interface ICompletable
    {
        #region Events

        event EventHandler<OperationEventArgs> OperationComplete;

        #endregion
    }
}
