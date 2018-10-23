using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;

namespace Ferretto.Common.Controls
{
    public class GraphicUtils
    {
        #region Methods

        public static double ConvertMillimetersToPixel(double value, double pixel, double mm, int offsetMM = 0)
        {
            if (mm > 0)
            {
                return (pixel * value) / mm + offsetMM;
            }
            return value;
        }

        public static double ConvertWithStandardOrigin(double value, PositionType positionType, Position trayOrigin, Dimension trayDimension, Dimension compartmentDimension)
        {
            //case: Origin X=0, Y=0
            if (trayOrigin.XPosition == 0 && trayOrigin.YPosition == 0) { }
            //case: Origin X=0, Y=Height
            if (trayOrigin.XPosition == 0 && trayOrigin.YPosition == trayDimension.Height)
            {
                if (positionType == PositionType.Y)
                {
                    value = trayOrigin.YPosition - value - compartmentDimension.Height;
                }
            }
            //case: Origin X=Width, Y=0
            if (trayOrigin.XPosition == trayDimension.Width && trayOrigin.YPosition == 0)
            {
                if (positionType == PositionType.X)
                {
                    value = trayDimension.Width - value - compartmentDimension.Width;
                }
            }
            //case: Origin X=Width, Y=Height
            if (trayOrigin.XPosition == trayDimension.Width && trayOrigin.YPosition == trayDimension.Height)
            {
                if (positionType == PositionType.X)
                {
                    value = trayDimension.Width - value - compartmentDimension.Width;
                }
                if (positionType == PositionType.Y)
                {
                    value = trayOrigin.YPosition - value - compartmentDimension.Height;
                }
            }
            return value;
        }

        #endregion Methods
    }
}
