using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Controls.WPF;
using Ferretto.WMS.App.Resources;
using Enums = Ferretto.Common.Resources.Enums;

namespace Ferretto.WMS.App.Core.Models
{
    public sealed class LoadingUnitDetails : BusinessObject
    {
        #region Fields

        private string abcClassId;

        private int? aisleId;

        private int? areaId;

        private IEnumerable<Enumeration> cellChoices;

        private int? cellId;

        private int? cellPositionId;

        private string code;

        private double depth;

        private int? handlingParametersCorrection;

        private double? height;

        private bool isCellPairingFixed;

        private string loadingUnitStatusId;

        private int? loadingUnitTypeId;

        private int missionsCount;

        private string note;

        private Enums.ReferenceType? referenceType;

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

        public IEnumerable<Enumeration> CellChoices
        {
            get => this.cellChoices;
            set => this.SetProperty(ref this.cellChoices, value);
        }

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
        [Display(Name = nameof(BusinessObjects.Code), ResourceType = typeof(BusinessObjects))]
        public string Code
        {
            get => this.code;
            set => this.SetProperty(ref this.code, value);
        }

        public BindingList<IDrawableCompartment> Compartments { get; } = new BindingList<IDrawableCompartment>();

        public int CompartmentsCount { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCreationDate), ResourceType = typeof(BusinessObjects))]
        public DateTime CreationDate { get; set; }

        [Display(Name = nameof(BusinessObjects.Depth), ResourceType = typeof(BusinessObjects))]
        public double Depth
        {
            get => this.depth;
            set => this.SetProperty(ref this.depth, value);
        }

        [Display(
                    Name = nameof(BusinessObjects.LoadingUnitHandlingParametersCorrection),
                    ResourceType = typeof(BusinessObjects))]
        public int? HandlingParametersCorrection
        {
            get => this.handlingParametersCorrection;
            set => this.SetProperty(ref this.handlingParametersCorrection, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.Height), ResourceType = typeof(BusinessObjects))]
        public double? Height
        {
            get => this.height;
            set => this.SetProperty(ref this.height, value);
        }

        [Display(Name = nameof(BusinessObjects.LastInventoryDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? InventoryDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitIsCellPairingFixed), ResourceType = typeof(BusinessObjects))]
        public bool IsCellPairingFixed
        {
            get => this.isCellPairingFixed;
            set => this.SetProperty(ref this.isCellPairingFixed, value);
        }

        [Display(Name = nameof(BusinessObjects.LoadingUnitLastHandlingDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastHandlingDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LastPickDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPickDate { get; set; }

        [Display(Name = nameof(BusinessObjects.LastPutDate), ResourceType = typeof(BusinessObjects))]
        public DateTime? LastPutDate { get; set; }

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

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitMissionsCount), ResourceType = typeof(BusinessObjects))]
        public int MissionsCount
        {
            get => this.missionsCount;
            set => this.SetProperty(ref this.missionsCount, value);
        }

        [Display(Name = nameof(BusinessObjects.Notes), ResourceType = typeof(BusinessObjects))]
        public string Note
        {
            get => this.note;
            set => this.SetProperty(ref this.note, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitReferenceType), ResourceType = typeof(BusinessObjects))]
        public Enums.ReferenceType? ReferenceType
        {
            get => this.referenceType;
            set => this.SetProperty(ref this.referenceType, value);
        }

        [Required]
        [Display(Name = nameof(BusinessObjects.LoadingUnitWeight), ResourceType = typeof(BusinessObjects))]
        public int? Weight
        {
            get => this.weight;
            set => this.SetProperty(ref this.weight, value);
        }

        [Display(Name = nameof(BusinessObjects.Width), ResourceType = typeof(BusinessObjects))]
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
                        return this.GetErrorMessageIfNegative(this.HandlingParametersCorrection, columnName);

                    case nameof(this.Height):
                        return this.GetErrorMessageIfNegativeOrZero(this.Height, columnName);

                    case nameof(this.Weight):
                        return this.GetErrorMessageIfNegativeOrZero(this.Weight, columnName);

                    case nameof(this.LoadingUnitTypeId):
                        return this.GetErrorMessageIfZeroOrNull(this.LoadingUnitTypeId, columnName);
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

            this.Compartments.Add(compartmentDetails);
        }

        #endregion
    }
}
