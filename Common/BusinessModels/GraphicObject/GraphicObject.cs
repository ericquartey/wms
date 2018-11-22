using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Ferretto.Common.Resources;

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

    public class BulkCompartment : INotifyPropertyChanged
    {
        #region Fields

        //private CompartmentDetails compartmentDetails;
        private int column;

        private int height;
        private int id;
        private int row;
        private int width;
        private int xPosition;
        private int yPosition;

        #endregion Fields

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        [Display(Name = nameof(BusinessObjects.BulkCompartmentColumn), ResourceType = typeof(BusinessObjects))]
        public int Column
        {
            get => this.column;
            set
            {
                this.column = value;
                this.NotifyPropertyChanged(nameof(this.Column));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public int Height
        {
            get => this.height;
            set
            {
                this.height = value;
                this.NotifyPropertyChanged(nameof(this.Height));
            }
        }

        //[Display(Name = nameof(BusinessObjects.com), ResourceType = typeof(BusinessObjects))]
        public int Id
        {
            get => this.id;
            set
            {
                this.id = value;
                this.NotifyPropertyChanged(nameof(this.Id));
            }
        }

        [Display(Name = nameof(BusinessObjects.BulkCompartmentRow), ResourceType = typeof(BusinessObjects))]
        public int Row
        {
            get => this.row;
            set
            {
                this.row = value;
                this.NotifyPropertyChanged(nameof(this.Row));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set
            {
                this.width = value;
                //this.SetIfStrictlyPositive(ref this.width, value);
                this.NotifyPropertyChanged(nameof(this.Width));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public int XPosition
        {
            get => this.xPosition;
            set
            {
                this.xPosition = value;

                this.NotifyPropertyChanged(nameof(this.XPosition));
            }
        }

        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public int YPosition
        {
            get => this.yPosition;
            set
            {
                this.yPosition = value;
                this.NotifyPropertyChanged(nameof(this.YPosition));
            }
        }

        #endregion Properties

        #region Methods

        //public CompartmentDetails CompartmentDetails
        //{
        //    get => this.compartmentDetails; set
        //    {
        //        this.compartmentDetails = value;
        //        this.NotifyPropertyChanged(nameof(this.CompartmentDetails));
        //    }
        //}
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
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

        public List<CompartmentDetails> BulkAddCompartments(BulkCompartment bulkCompartment)
        {
            var tempList = new List<CompartmentDetails>();
            int startX = (int)bulkCompartment.XPosition;
            int startY = (int)bulkCompartment.YPosition;
            for (int i = 0; i < bulkCompartment.Row; i++)
            {
                for (int j = 0; j < bulkCompartment.Column; j++)
                {
                    var newCompartment = new CompartmentDetails()
                    {
                        Width = bulkCompartment.Width,
                        Height = bulkCompartment.Height,
                        XPosition = startX + (i * bulkCompartment.Width),
                        YPosition = startY + (j * bulkCompartment.Height),
                    };
                    if (this.CanAddCompartment(newCompartment))
                    {
                        tempList.Add(newCompartment);
                    }
                    else
                    {
                        throw new ArgumentException();
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
                else
                {
                    if (bulkCompartment != null && bulkCompartment.Row > 1 && bulkCompartment.Column > 1)
                    {
                        //CompartmentDetails compartmentBulk = compartment.CompartmentDetails;
                        //CompartmentDetails details = compartment.CompartmentDetails;
                        bulkCompartment.Width = bulkCompartment.Width * bulkCompartment.Row;
                        bulkCompartment.Height = bulkCompartment.Height * bulkCompartment.Column;
                        //return this.CanAddCompartment(compartmentBulk);
                    }
                    else
                    {
                        //throw new ArgumentException();
                        //return false;
                    }
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
            var handler = CompartmentChangedEvent;
            if (handler != null)
            {
                handler(this, new CompartmentEventArgs(compartment));
            }
        }

        private string CanCreateNewCompartment(BulkCompartment bulkCompartment, Tray tray, bool isBulkAdd, bool edit = false)
        {
            //var details = bulkCompartment.CompartmentDetails;
            //if (details.XPosition != null)
            //{
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
                //if (areCollisions)
                //{
                //    return false;
                //}
            }
            return errors;
            //}
            //return true;
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
                error = "La larghezza del nuovo Scomparto non può essere minore o uguale a 0.";
                return error;
            }
            if (compartmentA.Height <= 0)
            {
                error = "L'altezza del nuovo Scomparto non può essere minore o uguale a 0.";
                return error;
            }
            if (compartmentA.XPosition < 0 || compartmentA.XPosition > tray.Dimension.Width)
            {
                error = "XPosition non può essere maggiore della larghezza del cassetto.";
                return error;
            }
            //}
            //if (compartmentA.YPosition != null)
            //{
            if (compartmentA.YPosition < 0 || compartmentA.YPosition > tray.Dimension.Height)
            {
                error = "XPosition non può essere maggiore dell'altezza del cassetto.";
                return error;
            }
            //}
            //if (compartmentA.XPosition != null && compartmentA.YPosition != null)
            //{
            //if (compartmentA.XPosition >= compartmentB.XPosition && compartmentA.YPosition >= compartmentB.YPosition
            //    )
            //{
            //    error = "XPosition e YPosition è in sovrapposizione con un altro Scomparto.";
            //    return error;
            //}
            //}
            //if (compartmentA.Width != null)
            //{
            if (compartmentA.Width < 0 || compartmentA.Width > tray.Dimension.Width)
            {
                error = "Width non può essere maggiore della larghezza del cassetto.";
                return error;
            }
            //}
            //if (compartmentA.Height != null)
            //{
            if (compartmentA.Height < 0 || compartmentA.Height > tray.Dimension.Height)
            {
                error = "Height non può essere maggiore dell'altezza del cassetto.";
                return error;
            }
            //}
            //if (compartmentA.XPosition != null && compartmentA.Width != null)
            //{
            if (compartmentA.XPosition + compartmentA.Width > tray.Dimension.Width)
            {
                error = "La dimensione del nuovo Scomparto non può essere maggiore della larghezza del cassetto.";
                return error;
            }
            //}
            //if (compartmentA.YPosition != null && compartmentA.Height != null)
            //{
            if (compartmentA.YPosition + compartmentA.Height > tray.Dimension.Width)
            {
                error = "La dimensione del nuovo Scomparto non può essere maggiore dell'altezza del cassetto.";
                return error;
            }

            CompartmentDetails compartmentDetailsA = this.ConvertBulkCompartmentToCompartmentDetails(compartmentA);
            bool areCollision = this.HasCollision(compartmentDetailsA, compartmentB);
            if (areCollision)
            {
                error = "Le dimensioni sono in sovrapposizione con un altro scomparto.";
                return error;
            }

            if (compartmentA.Row == 0 && isBulkAdd)
            {
                error = "Il numero di righe del nuovo Bulk Compartment non può essere minore o uguale a 0.";
                return error;
            }
            if (compartmentA.Column == 0 && isBulkAdd)
            {
                error = "Il numero di colonne del nuovo Bulk Compartment non può essere minore o uguale a 0.";
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
