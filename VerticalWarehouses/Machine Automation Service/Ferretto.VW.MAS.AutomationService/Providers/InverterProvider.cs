using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class InverterProvider : IInverterProvider
    {
        #region Fields

        private const int SkipCharsFromName = 4;

        private const int TotalInputs = 8;

        private const int WordSize = 16;

        private readonly InverterDriver.IInvertersProvider invertersProvider;

        #endregion

        #region Constructors

        public InverterProvider(InverterDriver.IInvertersProvider invertersProvider)
        {
            this.invertersProvider = invertersProvider;
        }

        #endregion

        #region Properties

        public IEnumerable<InverterDeviceInfo> GetStatuses => this.GetInvertersStatuses(this.invertersProvider.GetAll());

        #endregion

        #region Methods

        private static IEnumerable<BitInfo> GetBits(PropertyInfo[] properties, object status, int dimension, int skipCharFromName = 0)
        {
            var bits = Enumerable.Repeat(new BitInfo("NA", null, "NotUsed"), dimension).ToArray();
            foreach (var prop in properties)
            {
                if (prop.GetCustomAttribute<ColumnAttribute>() is ColumnAttribute colAttribute
                    &&
                    prop.CanRead
                    &&
                    prop.PropertyType == typeof(bool)
                    &&
                    prop.GetValue(status) is bool propValue)
                {
                    var position = colAttribute.Order;
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

            return GetBits(inverterInputsProperties, status, TotalInputs, SkipCharsFromName);
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
                device.ControlWords = GetBits(controlWordProperties, status.CommonControlWord, WordSize);
                device.StatusWords = GetBits(statusWordProperties, status.CommonStatusWord, WordSize);
                device.DigitalInputs = this.GetDigitalInputs(status);
                device.Id = (byte)status.SystemIndex;
                inverterDevices.Add(device);
            }

            return inverterDevices;
        }

        #endregion
    }
}
