using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.Data.Core.Models
{
    public class UnprocessableEntityOperationResult<T> : OperationResult<T>
    {
        #region Constructors

        public UnprocessableEntityOperationResult()
            : base(false)
        {
        }

        #endregion
    }
}
