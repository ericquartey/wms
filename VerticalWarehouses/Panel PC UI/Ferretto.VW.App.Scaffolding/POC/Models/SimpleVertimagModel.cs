using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class VertimagModel
    {
        [Editable(false)]
        public string Key { get; set; }

        // IEnumerables get categorized by some specific property (their very read-only value).
        [Category(ResourceType = typeof(POC.Resources.Vertimag), Category = nameof(POC.Resources.Vertimag.BayCategory))]
        [CategoryParameter(nameof(Bay.Number))]
        public IEnumerable<Bay> Bays { get; set; }
    }

    public class Bay
    {
        [PullToRoot]
        [Category(ResourceType = typeof(POC.Resources.Vertimag), Category = nameof(POC.Resources.Vertimag.BayInverterCategory))]
        [CategoryParameter(nameof(Bay.Number))]
        [Unfold]
        public Inverter Inverter { get; set; }

        [Category("Bay.{0} IoDevice")]
        [CategoryParameter(nameof(Bay.Number))]
        [PullToRoot]
        [Unfold]
        public IoDevice IoDevice { get; set; }

        [Editable(false)]
        public bool IsExternal { get; set; }

        [Editable(false)]
        public BayNumber Number { get; set; }

        [Range(0, double.PositiveInfinity, ErrorMessageResourceType = typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName = nameof(POC.Resources.ErrorMessages.RangeMin))]
        [DefaultValue(10), Unit("mm")]
        public double ChainOffset { get; set; }
    }

    public sealed class IoDevice
    {
        #region Properties

        public IoIndex Index { get; set; }

        [Required]
        public System.Net.IPAddress IpAddress { get; set; }

        [Required(ErrorMessageResourceType = typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName = nameof(POC.Resources.ErrorMessages.Required))]
        [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, ErrorMessageResourceType = typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName = nameof(POC.Resources.ErrorMessages.Range))]
        [DefaultValue(5001)]
        public int TcpPort { get; set; }

        #endregion Properties
    }

    [MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Inverter.Metadata))]
    public class Inverter
    {
        public System.Net.IPAddress IpAddress { get; set; }
        public int TcpPort { get; set; }

        private class Metadata
        {
            [Required]
            public System.Net.IPAddress IpAddress;

            [Required(ErrorMessageResourceType = typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName = nameof(POC.Resources.ErrorMessages.Required))]
            [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort, ErrorMessageResourceType = typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName = nameof(POC.Resources.ErrorMessages.Range))]
            [DefaultValue(5000)]
            public int TcpPort;
        }
    }

    public enum BayNumber : byte
    {
        BayOne = 1, BayTwo = 2
    }

    public enum IoIndex : byte
    {
        IoDevice1 = 0x00,

        IoDevice2 = 0x01,

        IoDevice3 = 0x02,

        All = 0x10,

        None = 0xFF,
    }
}
