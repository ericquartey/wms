using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.MAS.SocketLink
{
    internal interface ISocketLinkService
    {
        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        ///
        /// </summary>
        Task StopAsync();

        #endregion
    }
}
