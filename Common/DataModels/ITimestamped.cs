using System;

namespace Ferretto.Common.DataModels
{
    public interface ITimestamped
    {
        #region Properties

        DateTime CreationDate { get; set; }

        DateTime LastModificationDate { get; set; }

        #endregion
    }
}
