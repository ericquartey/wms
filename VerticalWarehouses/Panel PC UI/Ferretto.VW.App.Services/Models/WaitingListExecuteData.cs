using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Services
{
    public class WaitingListExecuteData
    {
        #region Properties

        public int AreaId { get; set; }

        public string AuthenticationUserName { get; set; }

        public int? BayId { get; set; }

        public string ListDescription { get; set; }

        public int ListId { get; set; }

        #endregion
    }
}
