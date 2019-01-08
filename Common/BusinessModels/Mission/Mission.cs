using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Ferretto.Common.Resources;

namespace Ferretto.Common.BusinessModels
{
    public class Mission : BusinessObject
    {
        #region Properties

        [Display(Name = nameof(BusinessObjects.Bay), ResourceType = typeof(BusinessObjects))]
        public string BayDescription { get; set; }

        public int? CellId { get; set; }

        public int? CompartmentId { get; set; }

        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        public string ItemDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        public string ItemListDescription { get; set; }

        [Display(Name = nameof(MasterData.ItemListRows), ResourceType = typeof(MasterData))]
        public string ItemListRowDescription { get; set; }

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        public string LoadingUnitDescription { get; set; }

        public string Lot { get; set; }

        public int? MaterialStatusId { get; set; }

        public int? PackageTypeId { get; set; }

        public int? Priority { get; set; }

        public string RegistrationNumber { get; set; }

        [Display(Name = nameof(General.Quantity), ResourceType = typeof(General))]
        public int RequiredQuantity { get; set; }

        [Display(Name = nameof(General.Status), ResourceType = typeof(General))]
        public MissionStatus Status { get; set; } = MissionStatus.New;

        public string Sub1 { get; set; }

        public string Sub2 { get; set; }

        [Display(Name = nameof(General.Type), ResourceType = typeof(General))]
        public MissionType Type { get; set; }

        #endregion Properties
    }
}
