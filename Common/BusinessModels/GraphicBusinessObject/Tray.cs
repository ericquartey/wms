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

        #endregion Fields

        #region Events

        public event EventHandler<CompartmentEventArgs> CompartmentChangedEvent;

        #endregion Events

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

        #endregion Properties

        #region Methods

        public void AddCompartment(ICompartment compartmentDetails)
        {
            if (this.CanAddCompartment(compartmentDetails))
            {
                this.compartments.Add(compartmentDetails);
            }
            else
            {
                throw new ArgumentException("Error adding new compartment because it overlaps with other compartments or it crosses the tray's boundaries.");
            }
        }

        public void AddCompartmentsRange(IList<ICompartment> compartmentDetails)
        {
            var error = false;
            foreach (var compartment in compartmentDetails)
            {
                //TODO: extreme check on compartment:
                //  1) bigger than tray
                //  2) over tray position
                this.compartments.Add(compartment);
            }

            if (error)
            {
                throw new ArgumentException("ERROR ADD NEW RANGE OF COMPARTMENTS: it is overlaps among other compartments or it exits from window.");
            }
        }

        public bool CanAddCompartment(ICompartment compartmentDetails, bool edit = false)
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
                if (edit && compartment.Id == compartmentDetails.Id)
                {
                    break;
                }
                var areCollisions = this.HasCollision(compartmentDetails, compartment);
                if (areCollisions)
                {
                    return false;
                }
            }
            return true;
        }

        public string CanBulkAddCompartment(object compartment, Tray tray, bool onPropertyChange, bool edit = false)
        {
            var isBulkAdd = true;
            if (compartment is CompartmentDetails details)
            {
                compartment = this.ConvertCompartmentDetailsToBulkCompartment(details);
                isBulkAdd = false;
            }

            if (compartment is BulkCompartment bulkCompartment)
            {
                if (onPropertyChange)
                {
                    return this.CanCreateNewCompartment(bulkCompartment, tray, isBulkAdd, edit);
                }
            }

            return null;
        }

        public void Update(CompartmentDetails compartment)
        {
            this.OnCompartmentChanged(compartment);
        }

        protected virtual void OnCompartmentChanged(CompartmentDetails compartment)
        {
            var handler = this.CompartmentChangedEvent;
            if (handler != null)
            {
                handler(this, new CompartmentEventArgs(compartment));
            }
        }

        private string CanCreateNewCompartment(BulkCompartment bulkCompartment, Tray tray, bool isBulkAdd, bool edit = false)
        {
            var errors = "";
            foreach (var compartment in this.compartments)
            {
                if (edit && compartment.Id == bulkCompartment.Id)
                {
                    break;
                }

                errors = this.HasCollisionSingleParameter(bulkCompartment, compartment, tray, isBulkAdd);
                if (errors == null || errors != "")
                {
                    return errors;
                }
            }
            return errors;
        }

        private CompartmentDetails ConvertBulkCompartmentToCompartmentDetails(BulkCompartment bulk)
        {
            var compartment = new CompartmentDetails();
            int row = 1, column = 1;
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

        private BulkCompartment ConvertCompartmentDetailsToBulkCompartment(CompartmentDetails compartment)
        {
            var bulk = new BulkCompartment();
            bulk.Width = compartment.Width ?? 0;
            bulk.Height = compartment.Height ?? 0;
            bulk.XPosition = compartment.XPosition ?? 0;
            bulk.YPosition = compartment.YPosition ?? 0;
            bulk.Id = compartment.Id;
            return bulk;
        }

        /// <summary>
        /// Checks if the specified compartments are physically overlapping.
        /// </summary>
        /// <returns>
        /// True if the specified compartments are overlapping, False otherwise.
        /// <returns>
        private bool HasCollision(ICompartment compartmentA, ICompartment compartmentB)
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

        private string HasCollisionSingleParameter(BulkCompartment compartmentA, ICompartment compartmentB, Tray tray, bool isBulkAdd)
        {
            var error = "";
            if (compartmentA.XPosition == 0 && compartmentA.YPosition == 0 && compartmentA.Width == 0 && compartmentA.Height == 0)
            {
                return null;
            }
            if (compartmentA.Width == 0 && compartmentA.Height == 0)
            {
                return null;
            }
            if (compartmentA.Width <= 0)
            {
                error = Errors.CompartmentWidthLess;
                return error;
            }
            if (compartmentA.Height <= 0)
            {
                error = Errors.CompartmentHeightLess;
                return error;
            }
            if (compartmentA.XPosition < 0 || compartmentA.XPosition > tray.Dimension.Width)
            {
                error = Errors.CompartmentXPosition;
                return error;
            }
            if (compartmentA.YPosition < 0 || compartmentA.YPosition > tray.Dimension.Height)
            {
                error = Errors.CompartmentYPosition;
                return error;
            }
            if (compartmentA.Width < 0 || compartmentA.Width > tray.Dimension.Width)
            {
                error = Errors.CompartmentWidthMore;
                return error;
            }
            if (compartmentA.Height < 0 || compartmentA.Height > tray.Dimension.Height)
            {
                error = Errors.CompartmentHeightMore;
                return error;
            }
            if (compartmentA.XPosition + compartmentA.Width > tray.Dimension.Width)
            {
                error = Errors.CompartmentSizeWMore;
                return error;
            }
            if (compartmentA.YPosition + compartmentA.Height > tray.Dimension.Width)
            {
                error = Errors.CompartmentSizeHMore;
                return error;
            }

            var compartmentDetailsA = this.ConvertBulkCompartmentToCompartmentDetails(compartmentA);
            var areCollision = this.HasCollision(compartmentDetailsA, compartmentB);
            if (areCollision)
            {
                error = Errors.CompartmentOverlaps;
                return error;
            }

            if (compartmentA.Rows == 0 && isBulkAdd)
            {
                error = Errors.BulkCompartmentRow;
                return error;
            }
            if (compartmentA.Columns == 0 && isBulkAdd)
            {
                error = Errors.BulkCompartmentColumn;
                return error;
            }

            return error;
        }

        #endregion Methods

        #region Classes

        public class CompartmentEventArgs : EventArgs
        {
            #region Fields

            private readonly CompartmentDetails compartment;

            #endregion Fields

            #region Constructors

            public CompartmentEventArgs(CompartmentDetails compartment)
            {
                this.compartment = compartment;
            }

            #endregion Constructors

            #region Properties

            public CompartmentDetails Compartment => this.compartment;

            #endregion Properties
        }

        #endregion Classes
    }
}
