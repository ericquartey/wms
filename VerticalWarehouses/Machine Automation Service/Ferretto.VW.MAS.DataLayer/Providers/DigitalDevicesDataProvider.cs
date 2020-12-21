using System.Collections.Generic;
using System.Linq;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.InverterDriver.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Ferretto.VW.MAS.DataLayer
{
    internal sealed class DigitalDevicesDataProvider : IDigitalDevicesDataProvider
    {
        #region Fields

        private readonly DataLayerContext dataContext;

        #endregion

        #region Constructors

        public DigitalDevicesDataProvider(DataLayerContext dataContext)
        {
            this.dataContext = dataContext ?? throw new System.ArgumentNullException(nameof(dataContext));
        }

        #endregion

        #region Methods

        public bool CheckInverterParametersValidity(InverterIndex index)
        {
            lock (this.dataContext)
            {
                if (this.dataContext.Inverters.Where(s => s.Index == index).Any())
                {
                    return !(this.dataContext.Inverters.Where(s => s.Index == index).FirstOrDefault().Parameters is null) &&
                        this.dataContext.Inverters.Where(s => s.Index == index).FirstOrDefault().Parameters.Any();
                }

                return false;
            }
        }

        public IEnumerable<Inverter> GetAllInverters()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Inverters.ToArray();
            }
        }

        public IEnumerable<Inverter> GetAllInvertersByBay(BayNumber bayNumber)
        {
            lock (this.dataContext)
            {
                if (bayNumber == BayNumber.ElevatorBay)
                {
                    return this.dataContext.Elevators
                        .Include(e => e.Axes)
                        .ThenInclude(a => a.Inverter)
                        .Single()
                        .Axes
                        .Select(a => a.Inverter)
                        .Where(i => i != null)
                        .ToList();
                }

                var bay = this.dataContext.Bays
                    .Include(b => b.Inverter)
                    .Include(b => b.Shutter)
                    .ThenInclude(s => s.Inverter)
                    .SingleOrDefault(b => b.Number == bayNumber);

                var inverters = new List<Inverter>();

                if (bay.Shutter != null && bay.Shutter.Inverter != null)
                {
                    inverters.Add(bay.Shutter.Inverter);
                }

                if (bay.Inverter != null)
                {
                    inverters.Add(bay.Inverter);
                }

                return inverters;
            }
        }

        public IEnumerable<IoDevice> GetAllIoDevices()
        {
            lock (this.dataContext)
            {
                var ioDevices = this.dataContext.IoDevices.ToArray();
                if (this.dataContext.Machines?.FirstOrDefault()?.Simulation ?? false)
                {
                    foreach (var io in ioDevices)
                    {
                        io.IpAddress = System.Net.IPAddress.Parse("127.0.0.1");
                        io.TcpPort += (int)io.Index;
                    }
                }
                return ioDevices;
            }
        }

        public IEnumerable<Inverter> GetAllParameters()
        {
            lock (this.dataContext)
            {
                return this.dataContext.Inverters.Include(i => i.Parameters).ToArray();
            }
        }

        public Inverter GetInverterByIndex(InverterIndex index)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.SingleOrDefault(i => i.Index == index);
                if (inverter is null)
                {
                    throw new EntityNotFoundException((int)index);
                }

                if (this.dataContext.Machines?.FirstOrDefault()?.Simulation ?? false)
                {
                    inverter.IpAddress = System.Net.IPAddress.Parse("127.0.0.1");
                }
                return inverter;
            }
        }

        public void UpdateInverterParameter(InverterIndex inverterIndex, short code, string value)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.SingleOrDefault(i => i.Index == inverterIndex);
                if (inverter is null)
                {
                    throw new EntityNotFoundException((int)inverterIndex);
                }

                var inverterParameter = inverter.Parameters.SingleOrDefault(p => p.Code == code);
                if (inverterParameter is null)
                {
                    throw new EntityNotFoundException(code);
                }

                inverterParameter.StringValue = value;
                this.dataContext.SaveChanges();
            }
        }

        #endregion
    }
}
