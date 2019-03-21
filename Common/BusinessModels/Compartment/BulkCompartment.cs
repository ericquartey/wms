using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class BulkCompartment : BusinessObject, ICompartment
    {
        #region Fields

        private const int MinGridSize = 1;

        private int columns = MinGridSize;

        private double? height;

        private int loadingUnitId;

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
            set => this.SetIfStrictlyPositive(ref this.columns, value);
        }

        public override string Error => string.Join(Environment.NewLine, new[]
            {
                this[nameof(this.XPosition)],
                this[nameof(this.YPosition)],
                this[nameof(this.Columns)],
                this[nameof(this.Rows)],
                this[nameof(this.Width)],
                this[nameof(this.Height)]
            }
          .Distinct()
          .Where(s => !string.IsNullOrEmpty(s)));

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentHeight), ResourceType = typeof(BusinessObjects))]
        public double? Height
        {
            get => this.height;
            set
            {
                if (this.SetProperty(ref this.height, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        public LoadingUnitDetails LoadingUnit { get; set; }

        public int LoadingUnitId
        {
            get => this.loadingUnitId;
            set => this.SetProperty(ref this.loadingUnitId, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.BulkCompartmentRows), ResourceType = typeof(BusinessObjects))]
        public int Rows
        {
            get => this.rows;
            set
            {
                if (this.SetProperty(ref this.rows, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentWidth), ResourceType = typeof(BusinessObjects))]
        public double? Width
        {
            get => this.width;
            set
            {
                if (this.SetProperty(ref this.width, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentXPosition), ResourceType = typeof(BusinessObjects))]
        public double? XPosition
        {
            get => this.xPosition;
            set
            {
                if (this.SetProperty(ref this.xPosition, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.CompartmentYPosition), ResourceType = typeof(BusinessObjects))]
        public double? YPosition
        {
            get => this.yPosition;
            set
            {
                if (this.SetProperty(ref this.yPosition, value))
                {
                    this.RaisePropertyChanged(nameof(this.Error));
                }
            }
        }

        private bool CanAddToLoadingUnit => this.LoadingUnit == null || this.LoadingUnit.CanAddCompartment(this);

        #endregion

        #region Indexers

        public override string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(this.XPosition):
                        if (this.XPosition < 0)
                        {
                            return Errors.CompartmentPositionCannotBeNegative;
                        }

                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentSetCannotBeInsertedInLoadingUnit;
                        }

                        break;

                    case nameof(this.YPosition):
                        if (this.YPosition < 0)
                        {
                            return Errors.CompartmentPositionCannotBeNegative;
                        }

                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentSetCannotBeInsertedInLoadingUnit;
                        }

                        break;

                    case nameof(this.Width):
                        if (this.Width <= 0)
                        {
                            return Errors.CompartmentSizeMustBeStrictlyPositive;
                        }

                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentSetCannotBeInsertedInLoadingUnit;
                        }

                        break;

                    case nameof(this.Height):
                        if (this.Height <= 0)
                        {
                            return Errors.CompartmentSizeMustBeStrictlyPositive;
                        }

                        if (this.CanAddToLoadingUnit == false)
                        {
                            return Errors.CompartmentSetCannotBeInsertedInLoadingUnit;
                        }

                        break;

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

                    default:
                        return string.Empty;
                }

                return string.Empty;
            }
        }

        #endregion

        #region Methods

        public IEnumerable<ICompartment> CreateBulk()
        {
            if (this.rows == 0 || this.columns == 0)
            {
                throw new InvalidOperationException();
            }

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
                        LoadingUnitId = this.LoadingUnitId
                    };

                    if (this.LoadingUnit.CanAddCompartment(compartment))
                    {
                        compartments.Add(compartment);
                    }
                    else
                    {
                        throw new InvalidOperationException(Errors.BulkAddNoPossible);
                    }
                }
            }

            return compartments;
        }

        #endregion
    }
}
