using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class GraphicUtils
    {
        #region Methods

        public static void AdjustAOriginBasedOnBorder(ref DoublePosition position, Tray tray)
        {
            //position = new DoublePosition();
            //case: Origin X=0, Y=0
            if (tray.Origin.X == 0 && tray.Origin.Y == 0)
            {
                //compartmentOrigin.Y += tray.BORDER_TRAY;
            }
            //case: Origin X=0, Y=Height
            if (tray.Origin.X == 0 && tray.Origin.Y == tray.Dimension.Height)
            {
                position.Y -= tray.BORDER_TRAY;
            }
            //case: Origin X=Width, Y=0
            if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == 0)
            {
                //position.X -= tray.DOUBLE_BORDER_TRAY;
            }
            //case: Origin X=Width, Y=Height
            if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == tray.Dimension.Height)
            {
                //position.X -= tray.DOUBLE_BORDER_TRAY;
                position.Y -= -tray.BORDER_TRAY;
            }
            //return position;
        }

        public static double ConvertMillimetersToPixel(double value, double pixel, double mm, int offsetMM = 0)
        {
            if (mm > 0)
            {
                return Math.Floor((pixel * value) / mm + offsetMM);
            }
            return value;
        }

        public static Thickness ConvertToBorderThickness(int value)
        {
            return new Thickness(value);
        }

        public static Position ConvertWithStandardOrigin(Position compartmentOrigin, Tray tray, Dimension compartmentDimension)
        {
            //case: Origin X=0, Y=0
            if (tray.Origin.X == 0 && tray.Origin.Y == 0)
            {
                //compartmentOrigin.Y += tray.BORDER_TRAY;
            }
            //case: Origin X=0, Y=Height
            if (tray.Origin.X == 0 && tray.Origin.Y == tray.Dimension.Height)
            {
                compartmentOrigin.Y = tray.Origin.Y - compartmentOrigin.Y - compartmentDimension.Height;// - tray.BORDER_TRAY;
            }
            //case: Origin X=Width, Y=0
            if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == 0)
            {
                compartmentOrigin.X = tray.Dimension.Width - compartmentOrigin.X - compartmentDimension.Width;// - tray.BORDER_TRAY;
            }
            //case: Origin X=Width, Y=Height
            if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == tray.Dimension.Height)
            {
                compartmentOrigin.X = tray.Dimension.Width - compartmentOrigin.X - compartmentDimension.Width;// - tray.BORDER_TRAY;
                compartmentOrigin.Y = tray.Origin.Y - compartmentOrigin.Y - compartmentDimension.Height;// - tray.BORDER_TRAY;
            }
            return compartmentOrigin;
        }

        public static Position ConvertWithStandardOriginPixel(Position compartmentOriginPixel, Tray tray, Dimension compartmentDimension,
            double widthTrayPixel, double heightTrayPixel, double widthCompartmentPixel, double heightCompartmentPixel)
        {
            //int widthTray = (int)Math.Round(widthTrayPixel);
            //int heightTray = (int)Math.Round(heightTrayPixel);
            //int widthCompartment = (int)Math.Round(widthCompartmentPixel);
            //int heightCompartment = (int)Math.Round(heightCompartmentPixel);
            //case: Origin X=0, Y=0
            if (tray.Origin.X == 0 && tray.Origin.Y == 0)
            {
            }
            //case: Origin X=0, Y=Height
            if (tray.Origin.X == 0 && tray.Origin.Y == tray.Dimension.Height)
            {
                compartmentOriginPixel.Y = (int)Math.Floor(heightTrayPixel - compartmentOriginPixel.Y - heightCompartmentPixel);// - tray.BORDER_TRAY;
            }
            //case: Origin X=Width, Y=0
            if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == 0)
            {
                compartmentOriginPixel.X = (int)Math.Floor(widthTrayPixel - compartmentOriginPixel.X - widthCompartmentPixel);// - tray.BORDER_TRAY;
            }
            //case: Origin X=Width, Y=Height
            if (tray.Origin.X == tray.Dimension.Width && tray.Origin.Y == tray.Dimension.Height)
            {
                compartmentOriginPixel.X = (int)Math.Floor(widthTrayPixel - compartmentOriginPixel.X - widthCompartmentPixel);// - tray.BORDER_TRAY;
                compartmentOriginPixel.Y = (int)Math.Floor(heightTrayPixel - compartmentOriginPixel.Y - heightCompartmentPixel);// - tray.BORDER_TRAY;
            }
            return compartmentOriginPixel;
        }

        public static double ConvertWithStandardOriginSingleValue(double value, PositionType positionType, Position trayOrigin, Dimension trayDimension, Dimension compartmentDimension)
        {
            //case: Origin X=0, Y=0
            if (trayOrigin.X == 0 && trayOrigin.Y == 0) { }
            //case: Origin X=0, Y=Height
            if (trayOrigin.X == 0 && trayOrigin.Y == trayDimension.Height)
            {
                if (positionType == PositionType.Y)
                {
                    value = trayOrigin.Y - value - compartmentDimension.Height;
                }
            }
            //case: Origin X=Width, Y=0
            if (trayOrigin.X == trayDimension.Width && trayOrigin.Y == 0)
            {
                if (positionType == PositionType.X)
                {
                    value = trayDimension.Width - value - compartmentDimension.Width;
                }
            }
            //case: Origin X=Width, Y=Height
            if (trayOrigin.X == trayDimension.Width && trayOrigin.Y == trayDimension.Height)
            {
                if (positionType == PositionType.X)
                {
                    value = trayDimension.Width - value - compartmentDimension.Width;
                }
                if (positionType == PositionType.Y)
                {
                    value = trayOrigin.Y - value - compartmentDimension.Height;
                }
            }
            return value;
        }

        #endregion Methods
    }
}
