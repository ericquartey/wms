using System;
using Ferretto.WMS.Data.Hubs.Models;

namespace Ferretto.WMS.Data.Core.Providers
{
    public class Notification
    {
        #region Properties

        public string ModelId { get; set; }

        public Type ModelType { get; set; }

        public HubEntityOperation OperationType { get; set; }

        #endregion
    }
}
