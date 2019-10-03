using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Ferretto.VW.CommonUtils.Enumerations;
using Ferretto.VW.MAS.AutomationService.Interfaces;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.Interface.Services;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.AutomationService.Provider
{
    public class InverterProvider : Interfaces.IInverterProvider
    {
        #region Fields

        private const int SKIP_CHARS_FROM_NAME = 4;

        private const int TOTAL_INPUTS = 8;

        private const int WORD_DIMENSION = 16;

        private readonly InverterDriver.Interface.Services.IInvertersProvider invertersProvider;

        #endregion

        #region Constructors

        public InverterProvider(InverterDriver.Interface.Services.IInvertersProvider invertersProvider)
        {
            this.invertersProvider = invertersProvider;
        }

        #endregion

        #region Properties

        public IEnumerable<InverterDeviceInfo> GetStatuses => this.GetInvertersStatuses(this.invertersProvider.GetAll());

        #endregion

        #region Methods

        private IEnumerable<BitInfo> GetBits(PropertyInfo[] properties, object status, int dimension, int skipCharFromName = 0)
        {
            var bits = Enumerable.Repeat(new BitInfo("NA", null, "NotUsed"), dimension).ToArray();
            foreach (var prop in properties)
            {
                if (prop.CanRead &&
                    prop.PropertyType == typeof(bool) &&
                    prop.GetValue(status) is bool propValue)
                {
                    var position = prop.GetCustomAttribute<ColumnAttribute>().Order;
                    var propName = prop.Name.Substring(skipCharFromName);
                    bits[position] = new BitInfo(propName, propValue, propName);
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
                case IAcuInverterStatus _:
                    inverterInputsProperties = typeof(IAcuInverterStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    break;

                case IAglInverterStatus _:
                    inverterInputsProperties = typeof(IAglInverterStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    break;

                case IAngInverterStatus _:
                    inverterInputsProperties = typeof(IAngInverterStatus).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
                    break;

                default:
                    break;
            }

            return this.GetBits(inverterInputsProperties, status, TOTAL_INPUTS, SKIP_CHARS_FROM_NAME);
        }

        private IEnumerable<InverterDeviceInfo> GetInvertersStatuses(IEnumerable<IInverterStatusBase> inverterStatuses)
        {
            var inverterDevices = new List<InverterDeviceInfo>();
            var controlWordProperties = typeof(IControlWord).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            var statusWordProperties = typeof(IStatusWord).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly);
            foreach (var status in inverterStatuses)
            {
                var device = new InverterDeviceInfo();
                device.Id = (int)status.SystemIndex;
                device.ControlWords = this.GetBits(controlWordProperties, status.CommonControlWord, WORD_DIMENSION);
                device.StatusWords = this.GetBits(statusWordProperties, status.CommonStatusWord, WORD_DIMENSION);
                device.DigitalInputs = this.GetDigitalInputs(status);
                device.Id = (byte)status.SystemIndex;
                inverterDevices.Add(device);
            }

            return inverterDevices;
        }

        #endregion
    }
}
