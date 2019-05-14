using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public class BulkCompartment : BusinessObject, IDrawableCompartment
    {
        #region Fields

        private const int MinGridSize = 1;

        private int columns = MinGridSize;

        private double? height;

        private int? loadingUnitId;

        private int rows = MinGridSize;

        private double? width;

        private double? xPosition;

        private double? yPosition;

        #endregion

        #region Properties

        [Required]
        [Display(Name = nameof(BusinessObjects.BulkCompartmentColumns), ResourceType = typeof(BusinessObjects))]
        public int Columns
        {
            get => this.columns;
            set => this.SetProperty(ref this.columns, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public double? Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        public LoadingUnitDetails LoadingUnit { get; set; }

        public int? LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.BulkCompartmentRows), ResourceType = typeof(BusinessObjects))]
        public int Rows
        {
            get => this.rows;
            set => this.SetProperty(ref this.rows, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public double? Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public double? XPosition
        {
            get => this.xPosition;
            set => this.SetProperty(ref this.xPosition, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public double? YPosition
        {
            get => this.yPosition;
            set => this.SetProperty(ref this.yPosition, value);
        }

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                if (!this.IsValidationEnabled)
                {
                    return null;
                }

                var baseError = base[columnName];
                if (!string.IsNullOrEmpty(baseError))
                {
                    return baseError;
                }

                switch (columnName)
                {
                    case nameof(this.XPosition):
                        return GetErrorMessageIfNegative(this.XPosition, nameof(this.XPosition));

                    case nameof(this.YPosition):
                        return GetErrorMessageIfNegative(this.YPosition, nameof(this.YPosition));

                    case nameof(this.Width):
                        return GetErrorMessageIfNegativeOrZero(this.Width, nameof(this.Width));

                    case nameof(this.Height):
                        return GetErrorMessageIfNegativeOrZero(this.Height, nameof(this.Height));

                    case nameof(this.Rows):
                        if (this.Rows < MinGridSize)
                        {
                            return Errors.CompartmentBulkMustHaveAtLeastOneRow;
                        }

                        break;

                    case nameof(this.Columns):
                        if (this.Columns < MinGridSize)
                        {
                            return Errors.CompartmentBulkMustHaveAtLeastOneColumn;
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public IEnumerable<IDrawableCompartment> CreateBulk()
        {
            if (this.rows == 0 || this.columns == 0)
            {
                throw new InvalidOperationException();
            }

            var compartments = new List<IDrawableCompartment>();

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
                        LoadingUnitId = this.LoadingUnitId
                    };

                    compartments.Add(compartment);
                }
            }

            return compartments;
        }

        #endregion
    }
}
