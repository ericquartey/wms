using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using Ferretto.Common.Controls.WPF;
using Ferretto.Common.Resources;
using Ferretto.Common.Utils;

namespace Ferretto.WMS.App.Core.Models
{
    [Resource(nameof(Data.WebAPI.Contracts.LoadingUnit))]
    public sealed class LoadingUnitDetails : BusinessObject
    {
        #region Fields

        private readonly BindingList<IDrawableCompartment> compartments = new BindingList<IDrawableCompartment>();

        private string abcClassId;

        private int? aisleId;

        private int? areaId;

        private int? cellId;

        private int? cellPositionId;

        private string code;

        private int? handlingParametersCorrection;

        private double? height;

        private int inCycleCount;

        private bool isCellPairingFixed;

        private double length;

        private string loadingUnitStatusId;

        private int? loadingUnitTypeId;

        private string note;

        private ReferenceType? referenceType;

        private int? weight;

        private double width;

        #endregion

        #region Properties

        public IEnumerable<EnumerationString> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassDescription { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId
        {
            get => this.abcClassId;
            set => this.SetProperty(ref this.abcClassId, value);
        }

        public int? AisleId
        {
            get => this.aisleId;
            set => this.SetProperty(ref this.aisleId, value);
        }

        public int? AreaId
        {
            get => this.areaId;
            set => this.SetProperty(ref this.areaId, value);
        }

        public string AreaName { get; set; }

        public IEnumerable<Enumeration> CellChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCurrentCell), ResourceType = typeof(BusinessObjects))]
        public int? CellId
        {
            get => this.cellId;
            set => this.SetProperty(ref this.cellId, value);
        }

        public IEnumerable<Enumeration> CellPositionChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPosition), ResourceType = typeof(BusinessObjects))]
        public string CellPositionDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPosition), ResourceType = typeof(BusinessObjects))]
        public int? CellPositionId
        {
            get => this.cellPositionId;
            set => this.SetProperty(ref this.cellPositionId, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code
        {
            get => this.code;
            set => this.SetProperty(ref this.code, value);
        }

        public BindingList<IDrawableCompartment> Compartments => this.compartments;

        public int CompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCreationDate), ResourceType = typeof(BusinessObjects))]
        public DateTime CreationDate { get; set; }

        [Display(
            Name = nameof(BusinessObjects.LoadingUnitHandlingParametersCorrection),
            ResourceType = typeof(BusinessObjects))]
        public int? HandlingParametersCorrection
        {
            get => this.handlingParametersCorrection;
            set => this.SetProperty(ref this.handlingParametersCorrection, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitHeight), ResourceType = typeof(BusinessObjects))]
        public double? Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        [Required]
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
        public double Length
        {
            get => this.length;
            set => this.SetProperty(ref this.length, value);
        }

        public IEnumerable<EnumerationString> LoadingUnitStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusDescription { get; set; }

        [Required]
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

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public int? LoadingUnitTypeId
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

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitOtherCycleCount), ResourceType = typeof(BusinessObjects))]
        public int OtherCycleCount { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitOutCycleCount), ResourceType = typeof(BusinessObjects))]
        public int OutCycleCount { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitReferenceType), ResourceType = typeof(BusinessObjects))]
        public ReferenceType? ReferenceType
        {
            get => this.referenceType;
            set => this.SetProperty(ref this.referenceType, value);
        }

        public IEnumerable<EnumerationString> ReferenceTypeChoices { get; set; }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitWeight), ResourceType = typeof(BusinessObjects))]
        public int? Weight
        {
            get => this.weight;
            set => this.SetProperty(ref this.weight, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWidth), ResourceType = typeof(BusinessObjects))]
        public double Width
        {
            get => this.width;
            set => this.SetProperty(ref this.width, value);
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
                    case nameof(this.HandlingParametersCorrection):
                        return GetErrorMessageIfNegative(this.HandlingParametersCorrection, nameof(this.HandlingParametersCorrection));

                    case nameof(this.Height):
                        if (this.height < 1)
                        {
                            return string.Format(Errors.PropertyMustBePositive, nameof(this.Height));
                        }

                        break;

                    case nameof(this.Weight):
                        if (this.weight < 1)
                        {
                            return string.Format(Errors.PropertyMustBePositive, nameof(this.Weight));
                        }

                        break;

                    case nameof(this.LoadingUnitTypeId):
                        if (this.LoadingUnitTypeId == 0)
                        {
                            return string.Format(Errors.PropertyMustHaveValue, nameof(this.LoadingUnitTypeId));
                        }

                        break;
                }

                return null;
            }
        }

        #endregion

        #region Methods

        public void AddCompartment(IDrawableCompartment compartmentDetails)
        {
            if (compartmentDetails == null)
            {
                throw new ArgumentNullException(nameof(compartmentDetails));
            }

            this.compartments.Add(compartmentDetails);
        }

        #endregion
    }
}
