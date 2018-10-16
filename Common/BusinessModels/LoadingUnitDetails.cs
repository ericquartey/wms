using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public sealed class LoadingUnitDetails : BusinessObject<int>
    {
        #region Fields

        private readonly List<CompartmentDetails> compartments = new List<CompartmentDetails>();
        private int length;
        private int width;

        #endregion Fields

        #region Constructors

        public LoadingUnitDetails()
        { }

        public LoadingUnitDetails(int id) : base(id)
        { }

        #endregion Constructors

        #region Events

        public event EventHandler AddedCompartmentEvent;

        #endregion Events

        #region Properties

        public IEnumerable<Enumeration<string>> AbcClassChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.AbcClass), ResourceType = typeof(BusinessObjects))]
        public string AbcClassId { get; set; }

        public IEnumerable<Enumeration<int>> CellPositionChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.CellPosition), ResourceType = typeof(BusinessObjects))]
        public int CellPositionId { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitCode), ResourceType = typeof(BusinessObjects))]
        public string Code { get; set; }

        public IEnumerable<CompartmentDetails> Compartments => this.compartments.AsReadOnly();

        [Display(Name = nameof(BusinessObjects.LoadingUnitLength), ResourceType = typeof(BusinessObjects))]
        public int Length
        {
            get => this.length;
            set => this.SetIfStrictlyPositive(ref this.length, value);
        }

        public IEnumerable<Enumeration<string>> LoadingUnitStatusChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitStatus), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitStatusId { get; set; }

        public IEnumerable<Enumeration<int>> LoadingUnitTypeChoices { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitType), ResourceType = typeof(BusinessObjects))]
        public int LoadingUnitTypeId { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnitWidth), ResourceType = typeof(BusinessObjects))]
        public int Width
        {
            get => this.width;
            set => this.SetIfStrictlyPositive(ref this.width, value);
        }

        #endregion Properties

        #region Methods

        public void AddCompartment(CompartmentDetails compartmentDetails)
        {
            if (this.CanAddCompartment(compartmentDetails))
            {
                this.compartments.Add(compartmentDetails);
            }
        }

        public void AddDynamicCompartments(int row, int column, int XPosition, int YPosition, int width, int height)
        {
            //TODO: add logic of dynamic scompartition
            //      n: is calculated number of compartment to add
            //      n: based on row/column
            var n = 0;
            for (var i = 0; i < n; i++)
            {
                this.AddCompartment(null);
            }
        }

        public bool CanAddCompartment(CompartmentDetails compartmentDetails)
        {
            //TODO: add logic if possible add new compartment
            //      return FALSE if exit from tray or overlaps other compartment
            return true;
        }

        public void OnAddedCompartmentEvent(EventArgs e)
        {
            var handler = AddedCompartmentEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #endregion Methods
    }
}
