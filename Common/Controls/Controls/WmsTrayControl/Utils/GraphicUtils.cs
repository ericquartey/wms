using System;
using System.Windows;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class GraphicUtils
    {
        #region Methods

        public static double ConvertMillimetersToPixel(double value, double pixel, double mm)
        {
            return mm > 0 ? Math.Floor(pixel * value / mm) : value;
        }

        public static Position ConvertWithStandardOrigin(
            Position compartmentOrigin,
            Tray tray,
            int widthCompartment,
            int heightCompartment)
        {
            var ret = new Position() { X = compartmentOrigin.X, Y = compartmentOrigin.Y };

            // case: Origin X=0, Y=0 -> do nothing
            // case: Origin X=0, Y=Height
            if (tray.Origin.X == 0 && tray.Origin.Y == tray.Dimension.Height)
            {
                ret.Y = tray.Dimension.Height - compartmentOrigin.Y - heightCompartment;
            }

            // case: Origin X=Width, Y=0
            else if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == 0)
            {
                ret.X = tray.Dimension.Width - compartmentOrigin.X - widthCompartment;
            }

            // case: Origin X=Width, Y=Height
            else if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == tray.Dimension.Height)
            {
                ret.X = tray.Dimension.Width - compartmentOrigin.X - widthCompartment;
                ret.Y = tray.Dimension.Height - compartmentOrigin.Y - heightCompartment;
            }

            return ret;
        }

        #endregion Methods
    }
}
