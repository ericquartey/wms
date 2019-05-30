using System;
using System.Collections.Generic;
using System.Text;
using Ferretto.Common.BLL.Interfaces.Models;

namespace Ferretto.WMS.Data.Core.Models
{
    public interface IMachineLiveData : IModel<int>
    {
        #region Properties

        MachineStatus Status { get; set; }

        #endregion
    }
}
