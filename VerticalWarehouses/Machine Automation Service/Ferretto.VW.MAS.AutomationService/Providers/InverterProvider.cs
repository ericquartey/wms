using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Microsoft.Extensions.Configuration;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService
{
    internal class InverterProvider : IInverterProvider
    {
        #region Fields

        private const int SkipCharsFromName = 4;

        private const int TotalInputs = 8;

        private const int WordSize = 16;

        private readonly IConfiguration configuration;

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private readonly IEventAggregator eventAggregator;

        private readonly InverterDriver.IInvertersProvider invertersProvider;

        #endregion

        #region Constructors

        public InverterProvider(
            IEventAggregator eventAggregator,
            InverterDriver.IInvertersProvider invertersProvider,
            IDigitalDevicesDataProvider digitalDevicesDataProvider,
            IConfiguration configuration)
        {
            this.eventAggregator = eventAggregator;
            this.invertersProvider = invertersProvider;
            this.digitalDevicesDataProvider = digitalDevicesDataProvider;
            this.configuration = configuration;
        }

        #endregion

        #region Properties

        public IEnumerable<InverterDeviceInfo> GetStatuses => GetInvertersStatuses(this.invertersProvider.GetAll());

        #endregion

        #region Methods

        public IEnumerable<Inverter> GetAllParameters()
        {
            return this.digitalDevicesDataProvider.GetAllParameters();
        }

        public void SaveInverterStructure(IEnumerable<Inverter> inverters)
        {
            foreach (var inverter in inverters)
            {
                this.digitalDevicesDataProvider.SaveInverterStructure(inverter);
                var notificationMessage = new NotificationMessage(
                               new InverterParametersMessageData(0, 0, $"Inverter {inverter.Index} structure updated", false),
                               $"Starting save inverter structure",
                               MessageActor.Any,
                               MessageActor.DeviceManager,
                               MessageType.InverterParameter,
                               BayNumber.All,
                               BayNumber.All,
                               MessageStatus.OperationUpdateData);
                this.eventAggregator?.GetEvent<NotificationEvent>().Publish(notificationMessage);
            }
        }

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

        private static IEnumerable<BitInfo> GetDigitalInputs(IInverterStatusBase status)
        {
            PropertyInfo[] inverterInputsProperties = null;
            switch (status)
            {
                case IAcuInverterStatus _:
                    inverterInputsProperties = typeof(IAcuInverterStatus).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    break;

                case IAglInverterStatus _:
                    inverterInputsProperties = typeof(IAglInverterStatus).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    break;

                case IAngInverterStatus _:
                    inverterInputsProperties = typeof(IAngInverterStatus).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    break;

                default:
                    break;
            }

            return GetBits(inverterInputsProperties, status, TotalInputs, SkipCharsFromName);
        }

        private static IEnumerable<InverterDeviceInfo> GetInvertersStatuses(IEnumerable<IInverterStatusBase> inverterStatuses)
        {
            var inverterDevices = new List<InverterDeviceInfo>();
            var controlWordProperties = typeof(IControlWord).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var statusWordProperties = typeof(IStatusWord).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            foreach (var status in inverterStatuses)
            {
                var device = new InverterDeviceInfo();
                device.Id = (int)status.SystemIndex;
                device.ControlWords = GetBits(controlWordProperties, status.CommonControlWord, WordSize);
                device.StatusWords = GetBits(statusWordProperties, status.CommonStatusWord, WordSize);
                device.DigitalInputs = GetDigitalInputs(status);
                device.Id = (byte)status.SystemIndex;
                inverterDevices.Add(device);
            }

            return inverterDevices;
        }

        #endregion
    }
}
