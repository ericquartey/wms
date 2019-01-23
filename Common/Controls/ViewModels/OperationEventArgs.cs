using System;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.Common.Controls
{
    public class OperationEventArgs : EventArgs
    {
        #region Constructors

        public OperationEventArgs(IBusinessObject model, bool isCanceled)
        {
            this.Model = model;
            this.IsCanceled = isCanceled;
        }

        #endregion Constructors

        #region Properties

        public bool IsCanceled { get; }

        public IBusinessObject Model { get; }

        #endregion Properties
    }
}
