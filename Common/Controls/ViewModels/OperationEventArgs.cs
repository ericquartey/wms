using System;
using Ferretto.Common.BLL.Interfaces.Base;

namespace Ferretto.Common.Controls
{
    public class OperationEventArgs : EventArgs
    {
        #region Constructors

        public OperationEventArgs(IModel<int> model, bool isCanceled)
        {
            this.Model = model;
            this.IsCanceled = isCanceled;
        }

        #endregion

        #region Properties

        public bool IsCanceled { get; }

        public IModel<int> Model { get; }

        #endregion
    }
}
