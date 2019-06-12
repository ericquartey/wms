using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BLL.Interfaces;

namespace Ferretto.WMS.App.Core
{
    public class MapModel : IMapModel
    {
        #region Constructors

        public MapModel(string argument, double? property)
        {
            this.Argument = argument;
            this.Value = property.HasValue ? property.Value : 0;
        }

        public MapModel(string argument, long? property)
        {
            this.Argument = argument;
            this.Value = property.HasValue ? property.Value : 0;
        }

        #endregion

        #region Properties

        public string Argument
        {
            get; set;
        }

        public double Value { get; set; }

        #endregion
    }
}
