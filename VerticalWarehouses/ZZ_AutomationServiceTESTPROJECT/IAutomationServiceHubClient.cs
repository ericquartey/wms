using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZZ_AutomationServiceTESTPROJECT
{
    internal interface IAutomationServiceHubClient
    {
        #region Methods

        Task ConnectAsync();

        #endregion Methods
    }
}
