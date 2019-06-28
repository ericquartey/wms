﻿namespace Ferretto.Common.BLL.Interfaces
{
    public interface IOperationResult<out TModel>
    {
        #region Properties

        string Description { get; }

        TModel Entity { get; }

        bool ShowToast { get; }

        bool Success { get; }

        #endregion
    }
}
