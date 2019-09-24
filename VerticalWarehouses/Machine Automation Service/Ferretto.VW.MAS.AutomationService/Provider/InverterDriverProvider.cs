using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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

        private IEnumerable<BitInfo> GetBits(PropertyInfo[] properties, object status, int dimension)
        {
            var bits = Enumerable.Repeat(new BitInfo("NA", null, "NotUsed"), dimension).ToArray();
            foreach (var prop in properties)
            {
                try
                {
                    if (prop.CanRead &&
                        prop.PropertyType == typeof(bool) &&
                        prop.GetValue(status) is bool propValue)
                    {
                        var position = prop.GetCustomAttribute<ColumnAttribute>().Order;
                        bits[position] = new BitInfo(prop.Name, propValue, prop.Name);
                    }
                }
                catch
                {
                }
            }

            return bits;
        }

        private IEnumerable<BitInfo> GetDigitalInputs(IInverterStatusBase status)
        {
            PropertyInfo[] inverterInputsProperties = null;
            var digitalInputs = new List<BitInfo>();
            switch (status)
            {
                case IAcuInverterStatus acu:
                    inverterInputsProperties = typeof(IAcuInverterStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    break;

                case IAglInverterStatus agl:
                    inverterInputsProperties = typeof(IAglInverterStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    break;

                case IAngInverterStatus ang:
                    inverterInputsProperties = typeof(IAngInverterStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    break;

                default:
                    break;
            }

            return this.GetBits(inverterInputsProperties, status, 8);
        }

        private IEnumerable<InverterDevice> GetInvertersStatuses(IEnumerable<IInverterStatusBase> inverterStatuses)
        {
            var inverterDevices = new List<InverterDevice>();
            var controlWordProperties = typeof(IControlWord).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            var statusWordProperties = typeof(IStatusWord).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var status in inverterStatuses)
            {
                var device = new InverterDevice();
                device.Id = (int)status.SystemIndex;
                device.ControlWords = this.GetBits(controlWordProperties, status.CommonControlWord, 16);
                device.StatusWords = this.GetBits(statusWordProperties, status.CommonStatusWord, 16);
                device.DigitalInputs = this.GetDigitalInputs(status);
                device.Id = status.SystemIndex;
                inverterDevices.Add(device);
            }

            return inverterDevices;
        }

        #endregion
    }
}
