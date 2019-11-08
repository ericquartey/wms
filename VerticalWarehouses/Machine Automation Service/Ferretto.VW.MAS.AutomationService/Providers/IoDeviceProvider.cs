using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.IODriver;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class IoDeviceProvider : IIoDeviceProvider
    {
        #region Fields

        private readonly IIoDevicesProvider ioDeviceService;

        #endregion

        #region Constructors

        public IoDeviceProvider(IIoDevicesProvider ioDeviceService)
        {
            this.ioDeviceService = ioDeviceService;
        }

        #endregion

        #region Properties

        public IEnumerable<IoDeviceInfo> GetStatuses => this.GetIoDevices(this.ioDeviceService.Devices);

        #endregion

        #region Methods

        private IEnumerable<IoDeviceInfo> GetIoDevices(IEnumerable<IoStatus> ioStatuses)
        {
            var ioDevices = new List<IoDeviceInfo>();
            var properties = typeof(IoStatus).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var status in ioStatuses)
            {
                var bits = Enumerable.Repeat(new BitInfo("NA", null, "NotUsed"), IoStatus.TotalInputs).ToArray();
                foreach (var prop in properties)
                {
                    var value = prop?.GetValue(status);
                    if (value is bool isValue &&
                        prop.GetCustomAttribute<ColumnAttribute>() is ColumnAttribute colAttribute)
                    {
                        var position = colAttribute.Order;
                        bits[position] = new BitInfo(prop.Name, isValue, prop.Name);
                    }
                }

                var pos = 0;
                var inputs = new List<BitInfo>();
                foreach (var inputData in status.InputData)
                {
                    inputs.Add(new BitInfo((pos++).ToString(), inputData, string.Empty));
                }

                pos = 0;
                var outputs = new List<BitInfo>();
                foreach (var outputData in status.OutputData)
                {
                    outputs.Add(new BitInfo((pos++).ToString(), outputData, string.Empty));
                }

                var device = new IoDeviceInfo();
                device.Id = (int)status.IoIndex;
                device.IoStatuses = bits;
                device.Inputs = inputs;
                device.Outputs = outputs;
                ioDevices.Add(device);
            }

            return ioDevices;
        }

        #endregion
    }
}
