using System.ComponentModel.DataAnnotations;

namespace Ferretto.Common.Resources.Enums
{
    public enum SchedulerRequestType
    {
        [Display(Name = nameof(BusinessObjects.EnumNotSpecified), ResourceType = typeof(BusinessObjects))]
        NotSpecified,

        [Display(Name = nameof(BusinessObjects.SchedulerRequestTypeItem), ResourceType = typeof(BusinessObjects))]
        Item = 'I',

        [Display(Name = nameof(BusinessObjects.SchedulerRequestTypeLoadingUnit), ResourceType = typeof(BusinessObjects))]
        LoadingUnit = 'U',

        [Display(Name = nameof(BusinessObjects.SchedulerRequestTypeItemList), ResourceType = typeof(BusinessObjects))]
        ItemList = 'L',

        [Display(Name = nameof(BusinessObjects.SchedulerRequestTypeItemListRow), ResourceType = typeof(BusinessObjects))]
        ItemListRow = 'R',
    }
}
