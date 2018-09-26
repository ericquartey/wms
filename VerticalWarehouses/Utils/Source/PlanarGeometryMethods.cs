using System.Windows;
using Ferretto.VW.CustomControls.Controls;

namespace Ferretto.VW.Utils.Source
{
    public static class PlanarGeometryMethods
    {
        #region Methods

        public static bool AreGivenCompartmentRectanglesOverlapping(Point compRect1Point1, Point compRect1Point2, Point compRect2Point1, Point compRect2Point2)
        {
            // If one rectangle is on left side of other
            if (compRect1Point1.X > compRect2Point2.X || compRect2Point1.X > compRect1Point2.X)
            {
                return false;
            }
            // If one rectangle is above other
            if (compRect1Point1.Y < compRect2Point2.Y || compRect2Point1.Y < compRect1Point2.Y)
            {
                return false;
            }
            return true;
        }

        public static Point CalculateCompartmentRectCenterPoint(CompartmentRectangle cr)
        {
            double x = cr.OriginX + (cr.Width / 2);
            double y = cr.OriginY + (cr.Height / 2);
            return new Point(x, y);
        }

        #endregion Methods
    }
}
