using System;
using System.Collections.Generic;
using System.Text;

namespace Ferretto.VW.MAS.DataModels
{
    public sealed class Customer : DataModel
    {
        #region Properties

        public string Address { get; set; }

        public string City { get; set; }

        public string Code { get; set; }

        public string Country { get; set; }

        public string Name { get; set; }

        public string Order { get; set; }

        public string Province { get; set; }

        public string Zip { get; set; }

        #endregion
    }
}
