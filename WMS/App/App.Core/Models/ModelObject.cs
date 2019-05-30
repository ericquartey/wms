using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.Common.Resources;

namespace Ferretto.WMS.App.Core.Models
{
    public enum ModelObject
    {
        [Display(Name = nameof(BusinessObjects.Item), ResourceType = typeof(BusinessObjects))]
        Item = 'I',

        [Display(Name = nameof(BusinessObjects.LoadingUnit), ResourceType = typeof(BusinessObjects))]
        LoadingUnit = 'U',

        [Display(Name = nameof(BusinessObjects.Cell), ResourceType = typeof(BusinessObjects))]
        Cell = 'C',

        [Display(Name = nameof(BusinessObjects.Compartment), ResourceType = typeof(BusinessObjects))]
        Compartment = 'O',

        [Display(Name = nameof(BusinessObjects.ItemList), ResourceType = typeof(BusinessObjects))]
        ItemList = 'L',

        [Display(Name = nameof(BusinessObjects.ItemListRow), ResourceType = typeof(BusinessObjects))]
        ItemListRow = 'R',

        [Display(Name = nameof(BusinessObjects.Mission), ResourceType = typeof(BusinessObjects))]
        Mission = 'M',

        [Display(Name = nameof(BusinessObjects.SchedulerRequest), ResourceType = typeof(BusinessObjects))]
        SchedulerRequest = 'Q'
    }
}
