using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ferretto.VW.App.Scaffolding.Models;
using Ferretto.VW.App.Scaffolding.Services;
using Ferretto.VW.MAS.Scaffolding.DataAnnotations;

namespace Ferretto.VW.App.Scaffolding.Design
{
    internal static class VertimagViewModel
    {
        public static readonly Models.VertimagModel Instance = new Models.VertimagModel
        {
            Bays = new[]
             {
                 new Models.Bay
                 {
                      Number = Models.BayNumber.BayOne,
                       ChainOffset = 10,
                        Inverter = new Models.Inverter
                        {
                             IpAddress = new System.Net.IPAddress(new byte[]{ 127,0,0,1}),
                              TcpPort = 3456
                        },
                         IoDevice = new Models.IoDevice{
                          IpAddress = new System.Net.IPAddress(new byte[]{192, 168, 0, 1 }),
                           TcpPort = 3457,
                            Index = Models.IoIndex.IoDevice1
                         }                         ,  IsExternal = true
                 },
                 new Models.Bay
                 {
                      Number = Models.BayNumber.BayTwo,
                       ChainOffset = 8,
                        Inverter = new Models.Inverter
                        {
                             IpAddress = new System.Net.IPAddress(new byte[]{ 192,168,0,2}),
                              TcpPort = 3458
                        },
                         IoDevice = new Models.IoDevice{
                          IpAddress = new System.Net.IPAddress(new byte[]{192, 168, 0, 3 }),
                           TcpPort = 3459,
                            Index = Models.IoIndex.IoDevice1
                         }                         ,  IsExternal = false
                 }
             },
            Key = "abcdefghijkl"
        };
    }

    public class VertimagStructuresViewModel : ObservableCollection<ScaffoldedStructure>
    {
        private static readonly ReadOnlyCollection<ScaffoldedStructure> _items;


        static VertimagStructuresViewModel()
        {
            _items = VertimagViewModel.Instance.Scaffold().Children;
        }

        public VertimagStructuresViewModel() : base(_items)
        {
        }
    }

    public class VertimagEntitiesViewModel : ObservableCollection<ScaffoldedEntity>
    {
        private static readonly ReadOnlyCollection<ScaffoldedEntity> _items;


        static VertimagEntitiesViewModel()
        {
            _items = VertimagViewModel.Instance.Scaffold().Entities;
        }

        public VertimagEntitiesViewModel() : base(_items)
        {
        }
    }

    public class Vertimag
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

        #endregion
    }

    [MAS.Scaffolding.DataAnnotations.MetadataType(typeof(Inverter.Metadata))]
    public class Inverter
    {
        public System.Net.IPAddress IpAddress { get; set; }
        public int TcpPort { get; set; }

        class Metadata
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
