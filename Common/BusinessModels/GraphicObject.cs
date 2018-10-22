using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.Common.BusinessModels
{
    public enum PositionType
    {
        X,
        Y
    }

    public class Dimension
    {
        #region Properties

        public int Height { get; set; }
        public int Width { get; set; }

        #endregion Properties
    }

    public class Position
    {
        #region Properties

        public int XPosition { get; set; }
        public int YPosition { get; set; }

        #endregion Properties
    }

    //internal class GraphicObject
    //{
    //}
}
