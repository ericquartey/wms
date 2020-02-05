using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(IoDevice.Metadata))]
    public partial class IoDevice
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Editable(false)]
            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.IoDevice_Index))]
            [Id(1)]
            public IoIndex Index { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.IoDevice_IpAddress))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [RegularExpression(Constants.IPAddressPattern, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Regex))]
            [Id(2)]
            public System.Net.IPAddress IpAddress { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.IoDevice_TcpPort))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            [Id(3)]
            public int TcpPort { get; set; }

            #endregion
        }

        #endregion
    }
}
