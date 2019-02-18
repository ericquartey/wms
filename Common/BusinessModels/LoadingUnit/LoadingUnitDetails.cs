using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class LoadingUnitDetails : BusinessObject
    {
        #region Fields

        private readonly BindingList<ICompartment> compartments = new BindingList<ICompartment>();

        private string abcClassId;

        private int aisleId;

        private int areaId;

        private int cellId;

        private int cellPositionId;

        private string code;

        private int? handlingParametersCorrection;

        private int height;

        private int inCycleCount;

        private bool isCellPairingFixed;

        private int length;

        private string loadingUnitStatusId;

        private int loadingUnitTypeId;

        private string note;

        private ReferenceType referenceType;

        private int weight;

        private int width;

        #endregion

        #region Properties

        public IEnumerable<EnumerationString> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId
        {
            get => this.abcClassId;
            set => this.SetProperty(ref this.abcClassId, value);
        }

        public int AisleId
        {
            get => this.aisleId;
            set => this.SetProperty(ref this.aisleId, value);
        }

        public int AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        public IEnumerable<Enumeration> CellChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCurrentCell), ResourceType = typeof(BusinessObjects))]
        public int CellId
        {
            get => this.cellId;
            set => this.SetProperty(ref this.cellId, value);
        }

        public IEnumerable<Enumeration> CellPositionChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPosition), ResourceType = typeof(BusinessObjects))]
        public string CellPositionDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPosition), ResourceType = typeof(BusinessObjects))]
        public int CellPositionId
        {
            get => this.cellPositionId;
            set => this.SetProperty(ref this.cellPositionId, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code
        {
            get => this.code;
            set => this.SetProperty(ref this.code, value);
        }

        public BindingList<ICompartment> Compartments => this.compartments;

        [Display(Name = nameof(BusinessObjects.LoadingUnitCreationDate), ResourceType = typeof(BusinessObjects))]
        public DateTime CreationDate { get; set; }

        [Display(
            Name = nameof(BusinessObjects.LoadingUnitHandlingParametersCorrection),
            ResourceType = typeof(BusinessObjects))]
        public int? HandlingParametersCorrection
        {
            get => this.handlingParametersCorrection;
            set => this.SetIfPositive(ref this.handlingParametersCorrection, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitHeight), ResourceType = typeof(BusinessObjects))]
        public int Height
        {
            get => this.height;
            set => this.SetIfStrictlyPositive(ref this.height, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitInCycleCount), ResourceType = typeof(BusinessObjects))]
        public int InCycleCount
        {
            get => this.inCycleCount;
            set => this.SetProperty(ref this.inCycleCount, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitIsCellPairingFixed), ResourceType = typeof(BusinessObjects))]
        public bool IsCellPairingFixed
        {
            get => this.isCellPairingFixed;
            set => this.SetProperty(ref this.isCellPairingFixed, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitLastHandlingDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastHandlingDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitLastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitLastStoreDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastStoreDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitLength), ResourceType = typeof(BusinessObjects))]
        public int Length
        {
            get => this.length;
            set => this.SetIfStrictlyPositive(ref this.length, value);
        }

        public IEnumerable<EnumerationString> LoadingUnitStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusId
        {
            get => this.loadingUnitStatusId;
            set => this.SetProperty(ref this.loadingUnitStatusId, value);
        }

        public IEnumerable<Enumeration> LoadingUnitTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitTypeDescription { get; set; }

        public bool LoadingUnitTypeHasCompartments { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public int LoadingUnitTypeId
        {
            get => this.loadingUnitTypeId;
            set => this.SetProperty(ref this.loadingUnitTypeId, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitNotes), ResourceType = typeof(BusinessObjects))]
        public string Note
        {
            get => this.note;
            set => this.SetProperty(ref this.note, value);
        }

        public Point OriginTray { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitOtherCycleCount), ResourceType = typeof(BusinessObjects))]
        public int OtherCycleCount { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitOutCycleCount), ResourceType = typeof(BusinessObjects))]
        public int OutCycleCount { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitReferenceType), ResourceType = typeof(BusinessObjects))]
        public ReferenceType ReferenceType
        {
            get => this.referenceType;
            set => this.SetProperty(ref this.referenceType, value);
        }

        public IEnumerable<EnumerationString> ReferenceTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWeight), ResourceType = typeof(BusinessObjects))]
        public int Weight
        {
            get => this.weight;
            set => this.SetIfPositive(ref this.weight, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set => this.SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion

        #region Methods

        public void AddCompartment(ICompartment compartmentDetails)
        {
            if (compartmentDetails == null)
            {
                throw new ArgumentNullException(nameof(compartmentDetails));
            }

            if (this.CanAddCompartment(compartmentDetails))
            {
                this.compartments.Add(compartmentDetails);
            }
            else
            {
                throw new ArgumentException(string.Format(Resources.Errors.LoadingUnitOverlappingCompartment, compartmentDetails.Id, this.Id));
            }
        }

        public bool CanAddCompartment(ICompartment compartment)
        {
            if (compartment == null)
            {
                throw new ArgumentNullException(nameof(compartment));
            }

            return
                (
                    this.LoadingUnitTypeHasCompartments
                    &&
                    compartment.XPosition + compartment.Width <= this.Width
                    &&
                    compartment.YPosition + compartment.Height <= this.Length
                    &&
                    !this.compartments.Any(c => HasCollision(c, compartment)))
                ||
                (
                    this.LoadingUnitTypeHasCompartments == false
                    &&
                    compartment.XPosition.HasValue == false
                    &&
                    compartment.YPosition.HasValue == false);
        }

        private static bool HasCollision(ICompartment c1, ICompartment c2)
        {
            if (c1.Id == c2.Id)
            {
                return false;
            }

            var xAPositionFinal = c1.XPosition + c1.Width;
            var yAPositionFinal = c1.YPosition + c1.Height;

            var xBPositionFinal = c2.XPosition + c2.Width;
            var yBPositionFinal = c2.YPosition + c2.Height;

            // A: Top-Left
            if (c1.XPosition >= c2.XPosition
                && c1.XPosition < xBPositionFinal
                && c1.YPosition >= c2.YPosition
                && c1.YPosition < yBPositionFinal)
            {
                return true;
            }

            // B: Top-Right
            if (xAPositionFinal > c2.XPosition
                && xAPositionFinal <= xBPositionFinal
                && c1.YPosition >= c2.YPosition
                && c1.YPosition < yBPositionFinal)
            {
                return true;
            }

            // C: Bottom-Left
            if (c1.XPosition >= c2.XPosition
                && c1.XPosition < xBPositionFinal
                && yAPositionFinal > c2.YPosition
                && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }

            // D: Bottom-Right
            if (xAPositionFinal > c2.XPosition
                && xAPositionFinal <= xBPositionFinal
                && yAPositionFinal > c2.YPosition
                && yAPositionFinal <= yBPositionFinal)
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
