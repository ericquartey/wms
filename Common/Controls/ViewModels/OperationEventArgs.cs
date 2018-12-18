using System;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class OperationEventArgs<T> : EventArgs
        where T : BusinessObject
    {
        #region Constructors

        public OperationEventArgs(T model)
        {
            this.Model = model;
        }

        #endregion Constructors

        #region Properties

        public T Model { get; private set; }

        #endregion Properties
    }
}
