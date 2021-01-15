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

        public void AddInverterParameter(InverterIndex inverterIndex, short code, int dataset, bool isReadOnly, string type, string value, string description, short writecode, short readcode, int decomalCount)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.SingleOrDefault(i => i.Index == inverterIndex);
                if (inverter is null)
                {
                    throw new EntityNotFoundException((int)inverterIndex);
                }

                var inverterParameter = new InverterParameter();

                inverterParameter.Code = code;
                inverterParameter.DataSet = dataset;
                inverterParameter.IsReadOnly = isReadOnly;
                inverterParameter.Type = type.ToLowerInvariant();
                inverterParameter.StringValue = value;
                inverterParameter.Description = description;
                inverterParameter.ReadCode = readcode;
                inverterParameter.WriteCode = writecode;
                inverterParameter.DecimalCount = decomalCount;

                var parameters = new List<InverterParameter>();

                if (!(inverter.Parameters is null) &&
                    inverter.Parameters.Any())
                {
                    parameters.AddRange(inverter.Parameters);
                    parameters.Add(inverterParameter);
                }
                else
                {
                    parameters.Add(inverterParameter);
                }

                inverter.Parameters = parameters;

                this.dataContext.Inverters.Update(inverter);
                this.dataContext.SaveChanges();
            }
        }

        public bool CheckInverterParametersValidity(InverterIndex index)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.Include(i => i.Parameters).SingleOrDefault(i => i.Index == index);
                if (inverter is null)
                {
                    throw new EntityNotFoundException((int)index);
                }

                return !(inverter.Parameters is null) &&
                    inverter.Parameters.Any();
            }
        }

        public bool ExistInverterParameter(InverterIndex inverterIndex, short code, int dataset)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.Include(i => i.Parameters).SingleOrDefault(i => i.Index == inverterIndex);
                if (inverter is null)
                {
                    throw new EntityNotFoundException((int)inverterIndex);
                }

                if (inverter.Parameters is null)
                {
                    return false;
                }

                return inverter.Parameters.Any(p => p.Code == code && p.DataSet == dataset);
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

        public InverterParameter GetParameter(InverterIndex inverterIndex, short code, int dataset)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.Include(i => i.Parameters).SingleOrDefault(i => i.Index == inverterIndex);
                return inverter.Parameters.FirstOrDefault(s => s.Code == code && s.DataSet == dataset);
            }
        }

        public void SaveInverterStructure(IEnumerable<Inverter> inverters)
        {
            lock (this.dataContext)
            {
                foreach (var inverter in inverters)
                {
                    var inverterDb = this.dataContext.Inverters.Include(i => i.Parameters).SingleOrDefault(i => i.Index == inverter.Index);
                    if (inverterDb is null)
                    {
                        throw new EntityNotFoundException((int)inverter.Index);
                    }
                    else
                    {
                        if (inverterDb.Parameters.Any())
                        {
                            this.dataContext.InverterParameter.RemoveRange(inverterDb.Parameters);
                            this.dataContext.SaveChanges();
                        }

                        inverterDb.Parameters = inverter.Parameters;

                        this.dataContext.Inverters.Update(inverterDb);
                        this.dataContext.SaveChanges();
                    }
                }
            }
        }

        public void UpdateInverterParameter(InverterIndex inverterIndex, short code, string value, int dataset)
        {
            lock (this.dataContext)
            {
                var inverter = this.dataContext.Inverters.Include(i => i.Parameters).SingleOrDefault(i => i.Index == inverterIndex);
                if (inverter is null)
                {
                    throw new EntityNotFoundException((int)inverterIndex);
                }

                var inverterParameter = inverter.Parameters.SingleOrDefault(p => p.Code == code && p.DataSet == dataset);
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
