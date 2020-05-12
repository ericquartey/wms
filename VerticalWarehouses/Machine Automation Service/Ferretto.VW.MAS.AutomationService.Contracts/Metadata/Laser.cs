using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.AutomationService.Contracts.Metadata.Resources;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.MAS.AutomationService.Contracts
{
    [Ferretto.VW.MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Laser.Metadata))]
    public partial class Laser
    {
        #region Classes

        private class Metadata
        {
            #region Properties

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Laser_IpAddress))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [RegularExpression(Constants.IPAddressPattern, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Regex))]
            [Id(1)]
            public System.Net.IPAddress IpAddress { get; set; }

            [Display(ResourceType = typeof(Vertimag), Name = nameof(Vertimag.Laser_TcpPort))]
            [Required(ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Required))]
            [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, ErrorMessageResourceType = typeof(ErrorMessages), ErrorMessageResourceName = nameof(ErrorMessages.Range))]
            [Id(2)]
            public int TcpPort { get; set; }

            #endregion
        }

        #endregion
    }
}
