using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.Installer.Services
{
    public class MasHealthCheckStep : IStep
    {
        #region Properties

        public StepStatus Status { get; set; }

        public string Title { get; set; }

        #endregion

        #region Methods

        public Task ApplyAsync()
        {
            throw new NotImplementedException();
        }

        public Task RollbackAsync()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
