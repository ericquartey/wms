using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Inverter.Metadata))]
    public partial class Inverter
    {
        private class Metadata
        {
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Inverter_Index))]
            public InverterIndex Index { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Inverter_IpAddress))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [RegularExpression(Constants.IPAddressPattern, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Regex))]
            public System.Net.IPAddress IpAddress { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Inverter_TcpPort))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            public int TcpPort { get; set; }

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Inverter_Type))]
            public InverterType Type { get; set; }
        }
    }
}
