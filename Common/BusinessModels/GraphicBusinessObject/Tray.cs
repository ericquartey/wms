using System;
using System.Collections.Generic;
using System.ComponentModel;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class Tray
    {
        #region Fields

        public readonly int DimensionRuler = 25;

        public readonly int DOUBLE_BORDER_TRAY = 2;

        private readonly BindingList<ICompartment> compartments = new BindingList<ICompartment>();

        private Dimension dimension;

        #endregion

        #region Events

        public event EventHandler<CompartmentEventArgs> CompartmentChangedEvent;

        #endregion

        #region Properties

        public BindingList<ICompartment> Compartments => this.compartments;

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

        public int LoadingUnitId { get; set; }

        public Position Origin { get; set; }

        public Dimension RulerSize { get; set; }

        #endregion

        #region Methods

        public void AddCompartment(ICompartment compartmentDetails)
        {
            if (this.CanAddCompartment(compartmentDetails))
            {
                this.compartments.Add(compartmentDetails);
            }
            else
            {
                throw new ArgumentException(Errors.CompartmentCannotBeInsertedInLoadingUnit);
            }
        }

        public void AddCompartmentsRange(IList<ICompartment> compartmentDetails)
        {
            var error = false;
            foreach (var compartment in compartmentDetails)
            {
                // TODO: extreme check on compartment:
                //  1) bigger than tray
                //  2) over tray position
                this.compartments.Add(compartment);
            }

            if (error)
            {
                throw new ArgumentException(Errors.CompartmentCannotBeInsertedInLoadingUnit);
            }
        }

        public bool CanAddCompartment(ICompartment compartmentDetails, bool edit = false)
        {
            // CHECK: exit from window
            var xPositionFinal = compartmentDetails.XPosition + compartmentDetails.Width;
            var yPositionFinal = compartmentDetails.YPosition + compartmentDetails.Height;
            if (xPositionFinal > this.Dimension.Width || yPositionFinal > this.Dimension.Height)
            {
                return false;
            }

            foreach (var compartment in this.compartments)
            {
                if (edit && compartment.Id == compartmentDetails.Id)
                {
                    break;
                }

                var areCollisions = HasCollision(compartmentDetails, compartment);
                if (areCollisions)
                {
                    return false;
                }
            }

            return true;
        }

        protected virtual void OnCompartmentChanged(CompartmentDetails compartment)
        {
            var handler = this.CompartmentChangedEvent;
            if (handler != null)
            {
                handler(this, new CompartmentEventArgs(compartment));
            }
        }

        /// <summary>
        /// Checks if the specified compartments are physically overlapping.
        /// </summary>
        /// <returns>
        /// True if the specified compartments are overlapping, False otherwise.
        /// </returns>
        private static bool HasCollision(ICompartment compartmentA, ICompartment compartmentB)
        {
            var xAPositionFinal = compartmentA.XPosition + compartmentA.Width;
            var yAPositionFinal = compartmentA.YPosition + compartmentA.Height;

            var xBPositionFinal = compartmentB.XPosition + compartmentB.Width;
            var yBPositionFinal = compartmentB.YPosition + compartmentB.Height;

            // A: Top-Left
            if (compartmentA.XPosition >= compartmentB.XPosition && compartmentA.XPosition < xBPositionFinal
                && compartmentA.YPosition >= compartmentB.YPosition && compartmentA.YPosition < yBPositionFinal)
            {
                return true;
            }

            // B: Top-Right
            if (xAPositionFinal > compartmentB.XPosition && xAPositionFinal <= xBPositionFinal
                && compartmentA.YPosition >= compartmentB.YPosition && compartmentA.YPosition < yBPositionFinal)
            {
                return true;
            }

            // C: Bottom-Left
            if (compartmentA.XPosition >= compartmentB.XPosition && compartmentA.XPosition < xBPositionFinal
                && yAPositionFinal > compartmentB.YPosition && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }

            // D: Bottom-Right
            if (xAPositionFinal > compartmentB.XPosition && xAPositionFinal <= xBPositionFinal
                && yAPositionFinal > compartmentB.YPosition && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }

            return false;
        }

        private CompartmentDetails ConvertBulkCompartmentToCompartmentDetails(BulkCompartment bulk)
        {
            var compartment = new CompartmentDetails();
            var row = 1;
            var column = 1;
            if (bulk.Rows != 0)
            {
                row = bulk.Rows;
            }

            if (bulk.Columns != 0)
            {
                column = bulk.Columns;
            }

            compartment.Width = bulk.Width * column;
            compartment.Height = bulk.Height * row;
            compartment.XPosition = bulk.XPosition;
            compartment.YPosition = bulk.YPosition;
            return compartment;
        }

        #endregion
    }
}
