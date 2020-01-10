using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using System.ComponentModel.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(IoDevice.Metadata))]
    public partial class IoDevice
    {
        class Metadata
        {
            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.IoDevice_Index))]
            public IoIndex Index { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.IoDevice_IpAddress))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [RegularExpression(Constants.IPAddressPattern, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Regex))]
            public System.Net.IPAddress IpAddress { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.IoDevice_TcpPort))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            public int TcpPort { get; set; }
        }
    }

}
