using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public enum DimensionType
    {
        Width,

        Height
    }

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

        #endregion
    }

    public class DoublePosition
    {
        #region Properties

        public double X { get; set; }

        public double Y { get; set; }

        #endregion
    }

    public class Line
    {
        #region Properties

        public double XEnd { get; set; }

        public double XStart { get; set; }

        public double YEnd { get; set; }

        public double YStart { get; set; }

        #endregion
    }

    public class Position
    {
        #region Properties

        public double X { get; set; }

        public double Y { get; set; }

        #endregion
    }
}
