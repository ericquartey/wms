using System.Collections.Generic;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.Interface.Services;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Provider
{
    public class InverterProvider : IInverterProvider
    {
        #region Fields

        private readonly IInverterService inverterService;

        #endregion

        #region Constructors

        public InverterProvider(IInverterService inverterService)
        {
            this.inverterService = inverterService;
        }

        #endregion

        #region Properties

        public IEnumerable<InverterDevice> GetStatuses => this.GetInvertersStatuses(this.inverterService.GetStatuses);

        #endregion

        #region Methods

        private IEnumerable<InverterDevice> GetInvertersStatuses(IEnumerable<IInverterStatusBase> inverterStatuses)
        {
            var inverterDevices = new List<InverterDevice>();
            var controlWordProperties = typeof(IControlWord).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            var statusWordProperties = typeof(IStatusWord).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var status in inverterStatuses)
            {
                var controlWords = new List<BitBase>();
                foreach (var prop in controlWordProperties)
                {
                    bool? value = null;
                    if (prop.CanRead &&
                         prop.GetValue(status.CommonControlWord) is bool propValue)
                    {
                        value = propValue;
                    }

                    controlWords.Add(new BitBase(prop.Name, value, prop.Name));
                }

                var statusWords = new List<BitBase>();
                foreach (var prop in statusWordProperties)
                {
                    bool? value = null;
                    if (prop.CanRead &&
                        prop.GetValue(status.CommonStatusWord) is bool propValue)
                    {
                        value = propValue;
                    }

                    statusWords.Add(new BitBase(prop.Name, value, prop.Name));
                }

                var inputs = new List<BitBase>();
                // TO DO retrieve input data

                var device = new InverterDevice();
                device.Id = (int)status.SystemIndex;
                device.ControlWords = controlWords;
                device.StatusWords = statusWords;
                device.DigitalIOs = inputs;
                device.Id = status.SystemIndex;
                inverterDevices.Add(device);
            }

            return inverterDevices;
        }

        #endregion
    }
}
