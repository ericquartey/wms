using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.IODriver;
using Ferretto.VW.MAS.IODriver.Interface.Services;

namespace Ferretto.VW.MAS.AutomationService.Provider
{
    public class IoDeviceProvider : IIoDeviceProvider
    {
        #region Fields

        private readonly IIoDeviceService ioDeviceService;

        #endregion

        #region Constructors

        public IoDeviceProvider(IIoDeviceService ioDeviceService)
        {
            this.ioDeviceService = ioDeviceService;
        }

        #endregion

        #region Properties

        public IEnumerable<IoDevice> GetStatuses => this.GetIoDevices(this.ioDeviceService.GetStatuses);

        #endregion

        #region Methods

        private IEnumerable<IoDevice> GetIoDevices(IEnumerable<IoStatus> ioStatuses)
        {
            var ioDevices = new List<IoDevice>();
            var properties = typeof(IoStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var status in ioStatuses)
            {
                var newData = new List<BitInfo>();
                foreach (var prop in properties)
                {
                    var value = prop?.GetValue(status);
                    if (value is bool isValue)
                    {
                        var bit = new BitInfo(prop.Name, isValue, prop.Name);
                        newData.Add(bit);
                    }
                }

                //var inputs = Enumerable.Repeat(new BitInfo("NA", null, "NotUsed"), 16).ToArray();
                var inputs = new List<BitInfo>();
                foreach (var inputData in status.InputData)
                {
                    inputs.Add(new BitInfo(string.Empty, inputData, string.Empty));
                }

                //var outputs = Enumerable.Repeat(new BitInfo("NA", null, "NotUsed"), 8).ToArray();
                var outputs = new List<BitInfo>();
                foreach (var outputData in status.OutputData)
                {
                    outputs.Add(new BitInfo(string.Empty, outputData, string.Empty));
                }

                var device = new IoDevice();
                device.Id = (int)status.IoIndex;
                device.IoStatuses = newData;
                device.Inputs = inputs;
                device.Outputs = outputs;
                ioDevices.Add(device);
            }

            return ioDevices;
        }

        #endregion
    }
}
