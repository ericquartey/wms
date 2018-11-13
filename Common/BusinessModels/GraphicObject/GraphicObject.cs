using System;
using System.Collections.Generic;
using System.ComponentModel;

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

        #endregion Properties
    }

    public class DoublePosition
    {
        #region Properties

        public double X { get; set; }
        public double Y { get; set; }

        #endregion Properties
    }

    public class Line
    {
        #region Properties

        public double XEnd { get; set; }
        public double XStart { get; set; }
        public double YEnd { get; set; }
        public double YStart { get; set; }

        #endregion Properties
    }

    public class Position
    {
        #region Properties

        public int X { get; set; }
        public int Y { get; set; }

        #endregion Properties
    }

    public class Tray
    {
        #region Fields

        public readonly int BORDER_TRAY = 1;
        public readonly int DimensionRuler = 25;
        public readonly int DOUBLE_BORDER_TRAY = 2;
        private readonly BindingList<CompartmentDetails> compartments = new BindingList<CompartmentDetails>();
        private Dimension dimension;

        #endregion Fields

        #region Properties

        public BindingList<CompartmentDetails> Compartments => this.compartments;

        public Dimension Dimension
        {
            get => this.dimension; set
            {
                this.dimension = value;
                this.RulerSize = new Dimension
                {
                    Width = this.dimension.Width + this.DimensionRuler,
                    Height = this.dimension.Height + this.DimensionRuler
                };
            }
        }

        public Position Origin { get; set; }

        public Dimension RulerSize { get; set; }

        #endregion Properties

        #region Methods

        public List<CompartmentDetails> AddBulkCompartments(CompartmentDetails compartment, int row, int column)
        //Position start, Dimension size, int row, int column, CompartmentDetails detail)
        {
            var tempList = new List<CompartmentDetails>();
            int startX = compartment.XPosition ?? 0;
            int startY = compartment.YPosition ?? 0;
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    var newCompartment = new CompartmentDetails()
                    {
                        Width = compartment.Width,
                        Height = compartment.Height,
                        XPosition = startX + (i * compartment.Width),
                        YPosition = startY + (j * compartment.Height),
                        ItemPairing = compartment.ItemPairing,
                        ItemCode = compartment.ItemCode,
                        Stock = compartment.Stock,
                        MaxCapacity = compartment.MaxCapacity,
                        CompartmentTypeId = compartment.CompartmentTypeId,
                        LoadingUnitId = compartment.LoadingUnitId
                    };
                    if (this.CanAddCompartment(newCompartment))
                    {
                        tempList.Add(newCompartment);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            this.AddCompartmentsRange(tempList);
            return tempList;
        }

        public void AddCompartment(CompartmentDetails compartmentDetails)
        {
            if (this.CanAddCompartment(compartmentDetails))
            {
                this.compartments.Add(compartmentDetails);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR ADD NEW COMPARTMENT: it is overlaps among other compartments or it exits from window.");
            }
        }

        public void AddCompartmentsRange(IList<CompartmentDetails> compartmentDetails)
        {
            bool error = false;
            foreach (var compartment in compartmentDetails)
            {
                //TODO: extreme check on compartment:
                //  1) bigger than tray
                //  2) over tray position
                this.compartments.Add(compartment);
            }

            if (error)
            {
                System.Diagnostics.Debug.WriteLine("ERROR ADD NEW RANGE OF COMPARTMENTS: it is overlaps among other compartments or it exits from window.");
            }
        }

        public bool CanAddCompartment(CompartmentDetails compartmentDetails)
        {
            //CHECK: exit from window
            var xPositionFinal = compartmentDetails.XPosition + compartmentDetails.Width;
            var yPositionFinal = compartmentDetails.YPosition + compartmentDetails.Height;
            if (xPositionFinal > this.Dimension.Width || yPositionFinal > this.Dimension.Height)
            {
                return false;
            }

            foreach (var compartment in this.compartments)
            {
                var areCollisions = this.HasCollision(compartmentDetails, compartment);
                if (areCollisions)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks if the specified compartments are physically overlapping.
        /// </summary>
        /// <returns>
        /// True if the specified compartments are overlapping, False otherwise.
        /// <returns>
        private bool HasCollision(CompartmentDetails compartmentA, CompartmentDetails compartmentB)
        {
            var xAPositionFinal = compartmentA.XPosition + compartmentA.Width;
            var yAPositionFinal = compartmentA.YPosition + compartmentA.Height;

            var xBPositionFinal = compartmentB.XPosition + compartmentB.Width;
            var yBPositionFinal = compartmentB.YPosition + compartmentB.Height;
            //A: Top-Left
            if (compartmentA.XPosition >= compartmentB.XPosition && compartmentA.XPosition < xBPositionFinal
                && compartmentA.YPosition >= compartmentB.YPosition && compartmentA.YPosition < yBPositionFinal)
            {
                return true;
            }
            //B: Top-Right
            if (xAPositionFinal > compartmentB.XPosition && xAPositionFinal <= xBPositionFinal
                && compartmentA.YPosition >= compartmentB.YPosition && compartmentA.YPosition < yBPositionFinal)
            {
                return true;
            }
            //C: Bottom-Left
            if (compartmentA.XPosition >= compartmentB.XPosition && compartmentA.XPosition < xBPositionFinal
                && yAPositionFinal > compartmentB.YPosition && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }
            //D: Bottom-Right
            if (xAPositionFinal > compartmentB.XPosition && xAPositionFinal <= xBPositionFinal
                && yAPositionFinal > compartmentB.YPosition && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }
            return false;
        }

        #endregion Methods
    }
}
