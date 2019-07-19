using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.WmsCommunication.Source
{
    public class DrawerActivityItemDetail
    {
        #region Properties

        public string Batch { get; set; }

        public string Image { get; set; }

        public string ItemCode { get; set; }

        public string ItemDescription { get; set; }

        public string ListCode { get; set; }

        public string ListDescription { get; set; }

        public string ListRow { get; set; }

        public string MaterialStatus { get; set; }

        public string PackageType { get; set; }

        public string PackingListCode { get; set; }

        public string PackingListDescription { get; set; }

        public string Position { get; set; }

        public string ProductionDate { get; set; }

        public string RequestedQuantity { get; set; }

        #endregion
    }
}
