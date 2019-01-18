using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundService
{
    public interface IAutomationServiceHub
    {
        #region Methods

        Task OnExecutingNewAction(string message);

        #endregion Methods
    }
}
