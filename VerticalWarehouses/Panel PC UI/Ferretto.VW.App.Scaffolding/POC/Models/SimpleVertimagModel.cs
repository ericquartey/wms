using Ferretto.VW.App.Scaffolding.DataAnnotations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ferretto.VW.App.Scaffolding.Models
{
    public class VertimagModel
    {
        [Editable(false)]
        public string Key { get; set; }

        // IEnumerables get categorized by some specific property (their very read-only value).
        [Category(CategoryResourceType =typeof(POC.Resources.Vertimag), CategoryResourceName =nameof(POC.Resources.Vertimag.BayCategory))]
        [CategoryParameter(nameof(Bay.Number))]
        public IEnumerable<Bay> Bays { get; set; }
    }

    public class Bay
    {
        [PullToRoot]
        [Category(CategoryResourceType = typeof(POC.Resources.Vertimag), CategoryResourceName = nameof(POC.Resources.Vertimag.BayInverterCategory))]
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

        public double ChainOffset { get; set; }
    }

    public sealed class IoDevice
    {
        #region Properties

        public IoIndex Index { get; set; }

        public System.Net.IPAddress IpAddress { get; set; }

        [Range(System.Net.IPEndPoint.MinPort, System.Net.IPEndPoint.MaxPort)]
        public int TcpPort { get; set; }

        #endregion
    }

    public class Inverter
    {
        [Required]
        public System.Net.IPAddress IpAddress { get; set; }

        [Required(ErrorMessageResourceType = typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName = nameof(POC.Resources.ErrorMessages.Required))]
        [Range(0, ushort.MaxValue, ErrorMessageResourceType =typeof(POC.Resources.ErrorMessages), ErrorMessageResourceName =nameof(POC.Resources.ErrorMessages.Range))]
        public int TcpPort { get; set; }
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
