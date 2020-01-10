using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer
{
    internal interface IStep
    {
        #region Properties

        StepStatus Status { get; }

        string Title { get; }

        #endregion

        #region Methods

        Task ApplyAsync();

        Task RollbackAsync();

        #endregion
    }
}
