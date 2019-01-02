using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class BulkCompartment : BusinessObject
    {
        #region Fields

        private int columns;
        private int height;
        private int rows;
        private int width;
        private int xPosition;
        private int yPosition;

        #endregion Fields

        #region Properties

        [Display(Name = nameof(BusinessObjects.BulkCompartmentColumns), ResourceType = typeof(BusinessObjects))]
        public int Columns
        {
            get => this.columns;
            set => this.SetIfStrictlyPositive(ref this.columns, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public int Height
        {
            get => this.height;
            set => this.SetIfStrictlyPositive(ref this.height, value);
        }

        public LoadingUnitDetails LoadingUnit { get; set; }

        [Display(Name = nameof(BusinessObjects.BulkCompartmentRows), ResourceType = typeof(BusinessObjects))]
        public int Rows
        {
            get => this.rows;
            set => this.SetIfStrictlyPositive(ref this.rows, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set => this.SetIfStrictlyPositive(ref this.width, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public int XPosition
        {
            get => this.xPosition;
            set => this.SetIfPositive(ref this.xPosition, value);
        }

        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public int YPosition
        {
            get => this.yPosition;
            set => this.SetIfPositive(ref this.yPosition, value);
        }

        #endregion Properties

        #region Methods

        public IEnumerable<ICompartment> CreateBulk()
        {
            var compartments = new List<ICompartment>();

            var widthNewCompartment = this.width / this.columns;
            var heightNewCompartment = this.height / this.rows;

            for (var r = 0; r < this.rows; r++)
            {
                for (var c = 0; c < this.columns; c++)
                {
                    var compartment = new CompartmentDetails
                    {
                        Width = widthNewCompartment,
                        Height = heightNewCompartment,
                        XPosition = this.XPosition + (c * widthNewCompartment),
                        YPosition = this.YPosition + (r * heightNewCompartment),
                    };

                    if (this.LoadingUnit.CanAddCompartment(compartment))
                    {
                        compartments.Add(compartment);
                    }
                    else
                    {
                        throw new Exception(Errors.BulkAddNoPossible);
                    }
                }
            }

            return compartments;
        }

        #endregion Methods
    }
}
