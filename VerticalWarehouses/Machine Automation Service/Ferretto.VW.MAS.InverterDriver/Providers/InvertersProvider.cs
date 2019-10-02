using System;
using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.MAS.DataLayer.Exceptions;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Ferretto.VW.MAS.InverterDriver.Interface.Services;
using Ferretto.VW.MAS.InverterDriver.InverterStatus;
using Ferretto.VW.MAS.InverterDriver.InverterStatus.Interfaces;
using Ferretto.VW.MAS.Utils.Events;
using Prism.Events;

namespace Ferretto.VW.MAS.InverterDriver.Services
{
    internal class InvertersProvider : IInvertersProvider
    {
        #region Fields

        private readonly IDigitalDevicesDataProvider digitalDevicesDataProvider;

        private IEnumerable<IInverterStatusBase> inverters;

        #endregion

        #region Constructors

        public InvertersProvider(
            IEventAggregator eventAggregator,
            IDigitalDevicesDataProvider digitalDevicesDataProvider)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (digitalDevicesDataProvider is null)
            {
                throw new ArgumentNullException(nameof(digitalDevicesDataProvider));
            }

            eventAggregator
                .GetEvent<NotificationEvent>()
                .Subscribe(
                    this.OnDataLayerReady,
                    ThreadOption.PublisherThread,
                    false,
                    message => message.Type == CommonUtils.Messages.Enumerations.MessageType.DataLayerReady);

            this.digitalDevicesDataProvider = digitalDevicesDataProvider;
        }

        #endregion

        #region Methods

        public IEnumerable<IInverterStatusBase> GetAll()
        {
            return this.inverters;
        }

        public IInverterStatusBase GetByIndex(InverterIndex index)
        {
            var inverter = this.inverters.SingleOrDefault(i => i.SystemIndex == index);

            if (inverter is null)
            {
                throw new EntityNotFoundException(index.ToString());
            }

            return inverter;
        }

        public IAngInverterStatus GetMainInverter()
        {
            System.Diagnostics.Debug.Assert(this.inverters.Any(i => i.SystemIndex == InverterIndex.MainInverter));

            return this.inverters.Single(i => i.SystemIndex == InverterIndex.MainInverter) as IAngInverterStatus;
        }

        private void OnDataLayerReady(NotificationMessage obj)
        {
            this.inverters = this.digitalDevicesDataProvider
             .GetAllInverters()
             .Select<DataModels.Inverter, IInverterStatusBase>(i =>
             {
                 switch (i.Type)
                 {
                     case DataModels.InverterType.Acu:
                         return new AcuInverterStatus(i.Index);

                     case DataModels.InverterType.Ang:
                         return new AngInverterStatus(i.Index);

                     case DataModels.InverterType.Agl:
                         return new AglInverterStatus(i.Index);

                     default:
                         throw new System.Exception();
                 }
             })
             .ToArray();

            if (this.inverters.SingleOrDefault(i => i.SystemIndex == InverterIndex.MainInverter) as IAngInverterStatus == null)
            {
                throw new System.Exception("No main inverter is configured in the system.");
            }
        }

        #endregion
    }
}
