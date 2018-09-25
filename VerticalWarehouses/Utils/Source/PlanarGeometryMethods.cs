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
            Point retPoint;
            double x = cr.OriginX + (cr.Width / 2);
            double y = cr.OriginY + (cr.Height / 2);

            retPoint = new Point(x, y);
            return retPoint;
        }

        public static bool CompartmentRectangleIntersectCompartmentRectangle(CompartmentRectangle cr0, CompartmentRectangle cr1)
        {
            Point cr0point1 = new Point(cr0.OriginX, cr0.OriginY); //top left
            Point cr0point2 = new Point(cr0.OriginX + cr0.Height, cr0.OriginY + cr0.Height); //bottom left
            Point cr0point3 = new Point(cr0.OriginX + cr0.Height + cr0.Width, cr0.OriginY + cr0.Height + cr0.Width); //bottom right
            Point cr0point4 = new Point(cr0.OriginX + cr0.Width, cr0.OriginY + cr0.Width); //top right

            Point cr1point1 = new Point(cr1.OriginX, cr1.OriginY); //top left
            Point cr1point2 = new Point(cr1.OriginX + cr1.Height, cr1.OriginY + cr1.Height); //bottom left
            Point cr1point3 = new Point(cr1.OriginX + cr1.Height + cr1.Width, cr1.OriginY + cr1.Height + cr1.Width); //bottom right
            Point cr1point4 = new Point(cr1.OriginX + cr1.Width, cr1.OriginY + cr1.Width); //top right

            if (cr0point1.X > cr1point2.X && cr0point1.X < cr1point3.X && cr1point2.Y > cr0point1.Y && cr1point2.Y < cr0point2.Y)
            {
                return true;
            }
            if (cr0point1.X > cr1point1.X && cr0point1.X < cr1point4.X && cr1point1.Y > cr0point1.Y && cr1point1.Y < cr0point2.Y)
            {
                return true;
            }
            if (cr0point3.X < cr1point3.X && cr0point3.X > cr1point2.X && cr1point2.Y > cr0point3.Y && cr1point2.Y < cr0point4.Y)
            {
                return true;
            }
            if (cr0point3.X < cr1point4.X && cr0point3.X > cr1point1.X && cr1point4.Y > cr0point3.Y && cr1point4.Y < cr0point4.Y)
            {
                return true;
            }
            if (cr1point1.X > cr0point2.X && cr1point1.X < cr0point3.X && cr0point2.Y > cr1point1.Y && cr0point2.Y < cr1point2.Y)
            {
                return true;
            }
            if (cr1point3.X > cr0point2.X && cr1point3.X < cr0point3.X && cr0point2.Y > cr1point4.Y && cr0point2.Y < cr1point3.Y)
            {
                return true;
            }
            if (cr1point1.X < cr0point4.X && cr1point1.X > cr0point1.X && cr0point4.Y > cr1point1.Y && cr0point4.Y < cr1point2.Y)
            {
                return true;
            }
            if (cr1point3.X < cr0point4.X && cr1point3.X > cr0point1.X && cr0point4.Y > cr1point4.Y && cr0point4.Y < cr1point3.Y)
            {
                return true;
            }
            return false;
        }

        public static bool LineIntersectsLine(Point line1point1, Point line1point2, Point line2point1, Point line2point2)
        {
            double q = (line1point1.Y - line2point1.Y) * (line2point2.X - line2point1.X) - (line1point1.X - line2point1.X) * (line2point2.Y - line2point1.Y);
            double d = (line1point2.X - line1point1.X) * (line2point2.Y - line2point1.Y) - (line1point2.Y - line1point1.Y) * (line2point2.X - line2point1.X);

            if (d == 0)
            {
                return false;
            }

            double r = q / d;

            q = (line1point1.Y - line2point1.Y) * (line1point2.X - line1point1.X) - (line1point1.X - line2point1.X) * (line1point2.Y - line1point1.Y);
            double s = q / d;

            return (r <= 0 || r >= 1 || s <= 0 || s >= 1) ? false : true;
        }

        public static bool LineIntersectsRect(Point p1, Point p2, CompartmentRectangle r)
        {
            return LineIntersectsLine(p1, p2, new Point(r.OriginX, r.OriginY), new Point(r.OriginX + r.Width, r.OriginY)) ||
                   LineIntersectsLine(p1, p2, new Point(r.OriginX + r.Width, r.OriginY), new Point(r.OriginX + r.Width, r.OriginY + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.OriginX + r.Width, r.OriginY + r.Height), new Point(r.OriginX, r.OriginY + r.Height)) ||
                   LineIntersectsLine(p1, p2, new Point(r.OriginX, r.OriginY + r.Height), new Point(r.OriginX, r.OriginY)) ||
                   (r.ContainsOrOnFrontier(p1) && r.ContainsOrOnFrontier(p2));
        }

        #endregion Methods
    }
}
