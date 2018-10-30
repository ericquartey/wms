using System;
using System.ComponentModel;
using System.Windows.Media;

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

    public interface IFilter
    {
        //Func<CompartmentDetails, Color> colorFunc;

        #region Properties

        Func<CompartmentDetails, Color> ColorFunc { get; }
        string Description { get; }
        int Id { get; }

        #endregion Properties
    };

    public class ArticleFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment)
        {
            Color color = Colors.Orange;
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, Color> ColorFunc { get => this.colorFunc; /*set => this.colorFunc = value;*/ }
        public string Description { get => "Article"; }
        public int Id { get => 1; }

        #endregion Properties
    }

    public class CompartmentFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment)
        {
            Color color = Colors.Red;
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, Color> ColorFunc { get => this.colorFunc; /*set => this.colorFunc = value;*/ }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }

        #endregion Properties
    }

    public class Dimension
    {
        #region Properties

        public int Height { get; set; }
        public int Width { get; set; }

        #endregion Properties
    }

    public class FillingFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment)
        {
            int stock = compartment.Stock;
            int? max = compartment.MaxCapacity;
            Color color = Colors.GreenYellow;

            if (max == null)
            {
                color = (Color)ColorConverter.ConvertFromString("#BDBDBD");
            }
            else
            {
                double filling = ((double)stock / (int)max) * 100;
                if (stock == 0 || (filling >= 0 && filling < 40))
                {
                    color = (Color)ColorConverter.ConvertFromString("#76FF03");
                }
                if (filling >= 40 && filling < 60)
                {
                    color = (Color)ColorConverter.ConvertFromString("#D4E157");
                }
                if (filling >= 60 && filling < 80)
                {
                    color = (Color)ColorConverter.ConvertFromString("#FF9800");
                }
                if (filling >= 80 && filling <= 99)
                {
                    color = (Color)ColorConverter.ConvertFromString("#F44336");
                }
                if (filling == 100)
                {
                    color = (Color)ColorConverter.ConvertFromString("#D50000");
                }
            }

            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, Color> ColorFunc { get => this.colorFunc; /*set => this.colorFunc = value;*/ }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }

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

    public class LinkedItemFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment)
        {
            Color color = Colors.Blue;
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, Color> ColorFunc { get => this.colorFunc; /*set => this.colorFunc = value;*/ }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }

        #endregion Properties
    }

    public class NotImplementdFilter : IFilter
    {
        #region Fields

        public Func<CompartmentDetails, Color> colorFunc = delegate (CompartmentDetails compartment)
        {
            Color color = Colors.Gray;
            return color;
        };

        #endregion Fields

        #region Properties

        public Func<CompartmentDetails, Color> ColorFunc { get => this.colorFunc; /*set => this.colorFunc = value;*/ }
        public string Description { get => "Compartment"; }
        public int Id { get => 2; }

        #endregion Properties
    }

    public class Position
    {
        #region Properties

        public int XPosition { get; set; }
        public int YPosition { get; set; }

        #endregion Properties
    }

    public class Tray
    {
        #region Fields

        public readonly int DimensionRuler = 25;
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

        public void AddDynamicCompartments(int row, int column, int XPosition, int YPosition, int width, int height)
        {
            //TODO: add logic of dynamic scompartition
            //      n: is calculated number of compartment to add
            //      n: based on row/column
            var n = 0;
            for (var i = 0; i < n; i++)
            {
                this.AddCompartment(null);
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
                bool areCollisions = this.HasCollision(compartmentDetails, compartment);
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
