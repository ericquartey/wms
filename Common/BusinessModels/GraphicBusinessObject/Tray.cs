using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class Tray
    {
        #region Fields

        public readonly int DimensionRuler = 25;
        public readonly int DOUBLE_BORDER_TRAY = 2;
        private readonly BindingList<CompartmentDetails> compartments = new BindingList<CompartmentDetails>();
        private Dimension dimension;

        #endregion Fields

        #region Events

        public event EventHandler<CompartmentEventArgs> CompartmentChangedEvent;

        #endregion Events

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

        public int LoadingUnitId { get; set; }
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
                throw new ArgumentException("ERROR ADD NEW COMPARTMENT: it is overlaps among other compartments or it exits from window.");
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
                throw new ArgumentException("ERROR ADD NEW RANGE OF COMPARTMENTS: it is overlaps among other compartments or it exits from window.");
            }
        }

        public List<CompartmentDetails> BulkAddCompartments(BulkCompartment bulkCompartment)
        {
            var tempList = new List<CompartmentDetails>();

            int widthNewCompartment = bulkCompartment.Width / bulkCompartment.Column;
            int heightNewCompartment = bulkCompartment.Height / bulkCompartment.Row;

            for (int i = 0; i < bulkCompartment.Row; i++)
            {
                for (int j = 0; j < bulkCompartment.Column; j++)
                {
                    var newCompartment = new CompartmentDetails()
                    {
                        Width = widthNewCompartment,
                        Height = heightNewCompartment,
                        XPosition = bulkCompartment.XPosition + (j * widthNewCompartment),
                        YPosition = bulkCompartment.YPosition + (i * heightNewCompartment),
                    };
                    if (this.CanAddCompartment(newCompartment))
                    {
                        tempList.Add(newCompartment);
                    }
                    else
                    {
                        throw new Exception(Errors.BulkAddNoPossible);
                    }
                }
            }

            this.AddCompartmentsRange(tempList);
            return tempList;
        }

        public bool CanAddCompartment(CompartmentDetails compartmentDetails, bool edit = false)
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
            bool isBulkAdd = true;
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
            CompartmentDetails compartment = new CompartmentDetails();
            int row = 1, column = 1;
            if (bulk.Row != 0)
            {
                row = bulk.Row;
            }
            if (bulk.Column != 0)
            {
                column = bulk.Column;
            }
            compartment.Width = bulk.Width * column;
            compartment.Height = bulk.Height * row;
            compartment.XPosition = bulk.XPosition;
            compartment.YPosition = bulk.YPosition;
            return compartment;
        }

        private BulkCompartment ConvertCompartmentDetailsToBulkCompartment(CompartmentDetails compartment)
        {
            BulkCompartment bulk = new BulkCompartment();
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

        private string HasCollisionSingleParameter(BulkCompartment compartmentA, CompartmentDetails compartmentB, Tray tray, bool isBulkAdd)
        {
            string error = "";
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

            CompartmentDetails compartmentDetailsA = this.ConvertBulkCompartmentToCompartmentDetails(compartmentA);
            bool areCollision = this.HasCollision(compartmentDetailsA, compartmentB);
            if (areCollision)
            {
                error = Errors.CompartmentOverlaps;
                return error;
            }

            if (compartmentA.Row == 0 && isBulkAdd)
            {
                error = Errors.BulkCompartmentRow;
                return error;
            }
            if (compartmentA.Column == 0 && isBulkAdd)
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

            public CompartmentDetails Compartment
            {
                get { return this.compartment; }
            }

            #endregion Properties
        }

        #endregion Classes
    }
}
